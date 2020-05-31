using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Infrastructure.DatabaseExtensions;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class AccountPaymentService : IAccountPaymentService
    {
        public AccountPaymentService(IAdministratorContext adminContext,
            IAccountPaymentProcessingService accountPaymentProcessingService,
            EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IServiceAccountContext serviceAccountContext,
            IAgentContext agentContext,
            IPaymentNotificationService notificationService,
            IAccountManagementService accountManagementService,
            ILogger<AccountPaymentService> logger)
        {
            _adminContext = adminContext;
            _accountPaymentProcessingService = accountPaymentProcessingService;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _serviceAccountContext = serviceAccountContext;
            _agentContext = agentContext;
            _accountManagementService = accountManagementService;
            _logger = logger;
            _notificationService = notificationService;
        }


        public async Task<bool> CanPayWithAccount(AgentInfo agentInfo)
        {
            var counterpartyId = agentInfo.CounterpartyId;
            return await _context.PaymentAccounts
                .Where(a => a.CounterpartyId == counterpartyId)
                .AnyAsync(a => a.Balance + a.CreditLimit > 0);
        }


        public async Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency)
        {
            var agent = await _agentContext.GetAgent();
            var accountInfo = await _context.PaymentAccounts
                .FirstOrDefaultAsync(a => a.Currency == currency && a.CounterpartyId == agent.CounterpartyId);
            
            return accountInfo == null
                ? Result.Failure<AccountBalanceInfo>($"Payments with accounts for currency {currency} is not available for current counterparty")
                : Result.Ok(new AccountBalanceInfo(accountInfo.Balance, accountInfo.CreditLimit, accountInfo.Currency));
        }


        public async Task<Result<string>> CaptureMoney(Booking booking, UserInfo user)
        {
            if (booking.PaymentMethod != PaymentMethods.BankTransfer)
                return Result.Failure<string>($"Invalid payment method: {booking.PaymentMethod}");

            return await Result.Ok(booking)
                .BindWithTransaction(_context, _ =>
                    CapturePayment()
                        .Tap(ChangePaymentStatusToCaptured)
                )
                .Finally(CreateResult);


            async Task<Result> CapturePayment()
            {
                var (_, isAccountFailure, account, accountError) = await GetAccount();
                if (isAccountFailure)
                    return Result.Failure(accountError);

                var (_, isFailure, paymentEntity, error) = await GetPayment(booking.Id);
                if (isFailure)
                    return Result.Failure<string>(error);

                return await CaptureAccountPayment()
                    .Tap(UpdatePaymentStatus);


                Task<Result<PaymentAccount>> GetAccount() => _accountManagementService.Get(booking.CounterpartyId, booking.Currency);


                async Task<Result> CaptureAccountPayment()
                {
                    // Hack. Error for updating same entity several times in different SaveChanges
                    _context.Detach(account);
                    var forVoid = booking.TotalPrice - paymentEntity.Amount;

                    var result = await _accountPaymentProcessingService.CaptureMoney(account.Id, new AuthorizedMoneyData(
                            currency: account.Currency,
                            amount: booking.TotalPrice,
                            referenceCode: booking.ReferenceCode,
                            reason: $"Capture money for the booking '{booking.ReferenceCode}' after check-in"),
                        user);

                    if (forVoid <= 0m || result.IsFailure)
                        return result;

                    _context.Detach(account);
                    return await _accountPaymentProcessingService.VoidMoney(account.Id, new AuthorizedMoneyData(
                            currency: account.Currency,
                            amount: forVoid,
                            referenceCode: booking.ReferenceCode,
                            reason: $"Void money for the booking '{booking.ReferenceCode}' after capture (booking was changed)"),
                        user);
                }


                async Task UpdatePaymentStatus()
                {
                    paymentEntity.Status = PaymentStatuses.Captured;
                    _context.Payments.Update(paymentEntity);
                    await _context.SaveChangesAsync();
                }
            }


            Task ChangePaymentStatusToCaptured() => ChangeBookingPaymentStatusToCaptured(booking);


            Result<string> CreateResult(Result result)
                => result.IsSuccess
                    ? Result.Ok($"Payment for the booking '{booking.ReferenceCode}' completed.")
                    : Result.Failure<string>($"Unable to complete payment for the booking '{booking.ReferenceCode}'. Reason: {result.Error}");
        }


        public Task<Result> ReplenishAccount(int accountId, PaymentData payment, Administrator administrator) => _accountPaymentProcessingService.AddMoney(accountId,
            payment,
            administrator.ToUserInfo());


        public Task<Result<PaymentResponse>> AuthorizeMoney(AccountBookingPaymentRequest request, AgentInfo agentInfo, string ipAddress)
        {
            return GetBooking()
                .BindWithTransaction(_context, booking =>
                    Authorize(booking)
                        .Tap(_ => ChangePaymentStatusToAuthorized(booking)));


            async Task<Result<Booking>> GetBooking()
            {
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.ReferenceCode == request.ReferenceCode);
                if (booking == null)
                    return Result.Failure<Booking>($"Could not find booking with reference code {request.ReferenceCode}");
                if (booking.AgentId != agentInfo.AgentId)
                    return Result.Failure<Booking>($"User does not have access to booking with reference code '{booking.ReferenceCode}'");

                return Result.Ok(booking);
            }


            async Task<Result<PaymentResponse>> Authorize(Booking booking)
            {
                var (_, isAmountFailure, amount, amountError) = await GetAmount();
                if (isAmountFailure)
                    return Result.Failure<PaymentResponse>(amountError);

                var (_, isAccountFailure, account, accountError) = await _accountManagementService.Get(agentInfo.CounterpartyId, booking.Currency);
                if (isAccountFailure)
                    return Result.Failure<PaymentResponse>(accountError);
               
                return await Result.Ok()
                    .Ensure(CanAuthorize, $"Could not authorize money for the booking '{booking.ReferenceCode}")
                    .Bind(AuthorizeMoney)
                    .Tap(StorePayment)
                    .Tap(SendBillToAgent)
                    .Map(CreateResult);


                Task<Result<decimal>> GetAmount() => GetPendingAmount(booking).Map(p => p.NetTotal);


                bool CanAuthorize()
                    => booking.PaymentMethod == PaymentMethods.BankTransfer &&
                        BookingStatusesForAuthorization.Contains(booking.Status);


                Task<Result> AuthorizeMoney()
                    => _accountPaymentProcessingService.AuthorizeMoney(account.Id, new AuthorizedMoneyData(
                            currency: account.Currency,
                            amount: amount,
                            reason: $"Authorize money after booking '{booking.ReferenceCode}'",
                            referenceCode: booking.ReferenceCode),
                        agentInfo.ToUserInfo());


                async Task StorePayment()
                {
                    var now = _dateTimeProvider.UtcNow();
                    var (_, isFailure, payment, _) = await GetPayment(booking.Id);
                    if (isFailure)
                    {
                        // New payment
                        var info = new AccountPaymentInfo(ipAddress);
                        payment = new Payment
                        {
                            Amount = amount,
                            BookingId = booking.Id,
                            AccountNumber = account.Id.ToString(),
                            Currency = booking.Currency.ToString(),
                            Created = now,
                            Modified = now,
                            Status = PaymentStatuses.Authorized,
                            Data = JsonConvert.SerializeObject(info),
                            AccountId = account.Id,
                            PaymentMethod = PaymentMethods.BankTransfer
                        };
                        _context.Payments.Add(payment);
                    }
                    else
                    {
                        // Partial payment
                        payment.Amount += amount;
                        payment.Modified = now;
                        _context.Payments.Update(payment);
                    }

                    await _context.SaveChangesAsync();
                }


                async Task SendBillToAgent()
                {
                    var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                    if (agent == default)
                    {
                        _logger.LogWarning("Send bill after payment from account: could not find agent with id '{0}' for the booking '{1}'", booking.AgentId,
                            booking.ReferenceCode);
                        return;
                    }

                    await _notificationService.SendBillToCustomer(new PaymentBill(agent.Email,
                        amount,
                        booking.Currency,
                        _dateTimeProvider.UtcNow(),
                        PaymentMethods.BankTransfer,
                        booking.ReferenceCode,
                        $"{agent.LastName} {agent.FirstName}"));
                }


                PaymentResponse CreateResult() => new PaymentResponse(string.Empty, CreditCardPaymentStatuses.Success, string.Empty);
            }


            async Task ChangePaymentStatusToAuthorized(Booking booking)
            {
                if (booking.PaymentStatus == BookingPaymentStatuses.Authorized)
                    return;

                booking.PaymentStatus = BookingPaymentStatuses.Authorized;
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<Result> VoidMoney(Booking booking, UserInfo user)
        {
            // TODO: Implement refund money if status is paid with deadline penalty
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Result.Ok();

            if (booking.PaymentMethod != PaymentMethods.BankTransfer)
                return Result.Failure($"Could not void money for the booking with a payment method  '{booking.PaymentMethod}'");

            return await GetAgent()
                .Bind(GetAccount)
                .Bind(VoidMoneyFromAccount);

            async Task<Result<AgentInfo>> GetAgent() => Result.Ok(await _agentContext.GetAgent());

            Task<Result<PaymentAccount>> GetAccount(AgentInfo agentInfo) => _accountManagementService.Get(agentInfo.CounterpartyId, booking.Currency);


            async Task<Result> VoidMoneyFromAccount(PaymentAccount account)
            {
                var (_, isFailure, paymentEntity, error) = await GetPayment(booking.Id);
                if (isFailure)
                    return Result.Failure(error);

                return await Void()
                    .Tap(UpdatePaymentStatus);


                Task<Result> Void() => _accountPaymentProcessingService.VoidMoney(account.Id, new AuthorizedMoneyData(paymentEntity.Amount, booking.Currency,
                    reason: $"Void money after booking cancellation '{booking.ReferenceCode}'", referenceCode: booking.ReferenceCode), user);


                async Task UpdatePaymentStatus()
                {
                    paymentEntity.Status = PaymentStatuses.Voided;
                    _context.Payments.Update(paymentEntity);
                    await _context.SaveChangesAsync();
                }
            }
        }


        public async Task<Result<Price>> GetPendingAmount(Booking booking)
        {
            if (booking.PaymentMethod != PaymentMethods.BankTransfer)
                return Result.Failure<Price>($"Unsupported payment method for pending payment: {booking.PaymentMethod}");

            var payment = await _context.Payments.Where(p => p.BookingId == booking.Id).FirstOrDefaultAsync();
            var paid = payment?.Amount ?? 0m;

            var forPay = booking.TotalPrice - paid;
            return forPay <= 0m
                ? Result.Failure<Price>("Nothing to pay")
                : Result.Ok(new Price(booking.Currency, forPay, forPay, PriceTypes.Supplement));
        }


        private Task ChangeBookingPaymentStatusToCaptured(Booking booking)
        {
            booking.PaymentStatus = BookingPaymentStatuses.Captured;
            _context.Bookings.Update(booking);
            return _context.SaveChangesAsync();
        }


        private async Task<Result<Payment>> GetPayment(int bookingId)
        {
            var paymentEntity = await _context.Payments.Where(p => p.BookingId == bookingId).FirstOrDefaultAsync();
            if (paymentEntity == default)
                return Result.Failure<Payment>(
                    $"Could not find a payment record with the booking ID {bookingId}");

            // Payment can be completed before. Nothing to do now.
            if (paymentEntity.Status != PaymentStatuses.Authorized)
                return Result.Failure<Payment>($"Invalid status for the payment entity with id '{paymentEntity.Id}': {paymentEntity.Status}");

            return Result.Ok(paymentEntity);
        }


        private static readonly HashSet<BookingStatusCodes> BookingStatusesForAuthorization = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.InternalProcessing
        };

        private readonly IAccountManagementService _accountManagementService;
        private readonly IAdministratorContext _adminContext;
        private readonly EdoContext _context;
        private readonly IAgentContext _agentContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AccountPaymentService> _logger;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IAccountPaymentProcessingService _accountPaymentProcessingService;
        private readonly IServiceAccountContext _serviceAccountContext;
    }
}