using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Infrastructure.DatabaseExtensions;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class AccountPaymentService : IAccountPaymentService
    {
        public AccountPaymentService(IAccountPaymentProcessingService accountPaymentProcessingService,
            EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IAccountManagementService accountManagementService,
            IEntityLocker locker)
        {
            _accountPaymentProcessingService = accountPaymentProcessingService;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _accountManagementService = accountManagementService;
            _locker = locker;
        }


        public async Task<bool> CanPayWithAccount(AgentContext agentInfo)
        {
            var agencyId = agentInfo.AgencyId;
            return await _context.AgencyAccounts
                .Where(a => a.AgencyId == agencyId && a.IsActive)
                .AnyAsync(a => a.Balance > 0);
        }


        public async Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, AgentContext agent)
        {
            var accountInfo = await _context.AgencyAccounts
                .FirstOrDefaultAsync(a => a.Currency == currency && a.AgencyId == agent.AgencyId && a.IsActive);

            return accountInfo == null
                ? Result.Failure<AccountBalanceInfo>($"Payments with accounts for currency {currency} is not available for current counterparty")
                : Result.Ok(new AccountBalanceInfo(accountInfo.Balance, accountInfo.Currency));
        }


        public async Task<Result<string>> Capture(Booking booking, UserInfo user)
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

                Task<Result<AgencyAccount>> GetAccount() => _accountManagementService.Get(booking.AgencyId, booking.Currency);


                async Task<Result> CaptureAccountPayment()
                {
                    // Hack. Error for updating same entity several times in different SaveChanges
                    _context.Detach(account);
                    return await _accountPaymentProcessingService.CaptureMoney(account.Id, new ChargedMoneyData(
                            currency: account.Currency,
                            amount: booking.TotalPrice,
                            referenceCode: booking.ReferenceCode,
                            reason: $"Capture money for the booking '{booking.ReferenceCode}' after check-in"),
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


        public Task<Result<PaymentResponse>> Authorize(AccountBookingPaymentRequest request, AgentContext agentInfo, string ipAddress)
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

                var (_, isAccountFailure, account, accountError) = await _accountManagementService.Get(agentInfo.AgencyId, booking.Currency);
                if (isAccountFailure)
                    return Result.Failure<PaymentResponse>(accountError);

                return await Result.Ok()
                    .Ensure(CanAuthorize, $"Could not authorize money for the booking '{booking.ReferenceCode}")
                    .Bind(AuthorizeMoney)
                    .Bind(StorePayment)
                    .Map(CreateResult);

                Task<Result<decimal>> GetAmount() => GetPendingAmount(booking).Map(p => p.NetTotal);


                bool CanAuthorize()
                    => booking.PaymentMethod == PaymentMethods.BankTransfer &&
                        BookingStatusesForAuthorization.Contains(booking.Status);


                Task<Result> AuthorizeMoney()
                    => _accountPaymentProcessingService.AuthorizeMoney(account.Id, new ChargedMoneyData(
                            currency: account.Currency,
                            amount: amount,
                            reason: $"Authorize money after booking '{booking.ReferenceCode}'",
                            referenceCode: booking.ReferenceCode),
                        agentInfo.ToUserInfo());


                async Task<Result> StorePayment()
                {
                    var (paymentExistsForBooking, _, _, _) = await GetPayment(booking.Id);
                    if (paymentExistsForBooking)
                        return Result.Failure("Payment for current booking already exists");

                    var now = _dateTimeProvider.UtcNow();
                    var info = new AccountPaymentInfo(ipAddress);
                    var payment = new Payment
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
                    await _context.SaveChangesAsync();

                    return Result.Success();
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

            return await GetAccount()
                .Bind(VoidMoneyFromAccount);


            Task<Result<AgencyAccount>> GetAccount() => _accountManagementService.Get(booking.AgencyId, booking.Currency);


            async Task<Result> VoidMoneyFromAccount(AgencyAccount account)
            {
                var (_, isFailure, paymentEntity, error) = await GetPayment(booking.Id);
                if (isFailure)
                    return Result.Failure(error);

                return await Void()
                    .Tap(UpdatePaymentStatus);


                Task<Result> Void()
                    => _accountPaymentProcessingService.VoidMoney(account.Id, new ChargedMoneyData(paymentEntity.Amount, booking.Currency,
                        reason: $"Void money after booking cancellation '{booking.ReferenceCode}'", referenceCode: booking.ReferenceCode), user);


                async Task UpdatePaymentStatus()
                {
                    paymentEntity.Status = PaymentStatuses.Voided;
                    _context.Payments.Update(paymentEntity);
                    await _context.SaveChangesAsync();
                }
            }
        }


        public async Task<Result> Refund(Booking booking, UserInfo user)
        {
            // TODO: Implement refund money if status is paid with deadline penalty
            if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                return Result.Ok();

            if (booking.PaymentMethod != PaymentMethods.BankTransfer)
                return Result.Failure($"Could not refund money for the booking with a payment method  '{booking.PaymentMethod}'");

            return await GetAccount()
                .Bind(RefundMoneyToAccount);


            Task<Result<AgencyAccount>> GetAccount() => _accountManagementService.Get(booking.AgencyId, booking.Currency);


            async Task<Result> RefundMoneyToAccount(AgencyAccount account)
            {
                var (_, isFailure, paymentEntity, error) = await GetPayment(booking.Id);
                if (isFailure)
                    return Result.Failure(error);

                return await Refund()
                    .Tap(UpdatePaymentStatus);


                Task<Result> Refund()
                    => _accountPaymentProcessingService.RefundMoney(account.Id, new ChargedMoneyData(paymentEntity.Amount, booking.Currency,
                        reason: $"Refund money after booking cancellation '{booking.ReferenceCode}'", referenceCode: booking.ReferenceCode), user);


                async Task UpdatePaymentStatus()
                {
                    paymentEntity.Status = PaymentStatuses.Refunded;
                    _context.Payments.Update(paymentEntity);
                    await _context.SaveChangesAsync();
                }
            }
        }


        public Task<Result<PaymentResponse>> Charge(string referenceCode, AgentContext agentContext, string clientIp)
        {
            return GetBooking()
                .Bind(b => Charge(b, agentContext, clientIp));


            async Task<Result<Booking>> GetBooking()
            {
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.ReferenceCode == referenceCode);
                if (booking == null)
                    return Result.Failure<Booking>($"Could not find booking with reference code {referenceCode}");
                if (!agentContext.IsUsingAgency(booking.AgencyId))
                    return Result.Failure<Booking>($"The booking with reference code '{booking.ReferenceCode}' does not belong to your current agency");

                return Result.Ok(booking);
            }
        }


        public Task<Result<PaymentResponse>> Charge(Booking booking, AgentContext agentContext, string clientIp)
        {
            return Result.Success()
                .BindWithTransaction(_context, () =>
                    Charge()
                        .Tap(_ => ChangePaymentStatusToCaptured()));


            async Task<Result<PaymentResponse>> Charge()
            {
                var (_, isAmountFailure, amount, amountError) = await GetAmount();
                if (isAmountFailure)
                    return Result.Failure<PaymentResponse>(amountError);

                var (_, isAccountFailure, account, accountError) = await _accountManagementService.Get(agentContext.AgencyId, booking.Currency);
                if (isAccountFailure)
                    return Result.Failure<PaymentResponse>(accountError);

                return await Result.Success()
                    .BindWithLock(_locker, typeof(Booking), booking.Id.ToString(), () => Result.Success()
                        .Ensure(IsNotPayed, $"The booking '{booking.ReferenceCode}' is already paid")
                        .Ensure(CanCharge, $"Could not charge money for the booking '{booking.ReferenceCode}'")
                        .Bind(ChargeMoney)
                        .Bind(StorePayment)
                        .Map(CreateResult));

                Task<Result<decimal>> GetAmount() => GetPendingAmount(booking).Map(p => p.NetTotal);

                bool IsNotPayed() => booking.PaymentStatus != BookingPaymentStatuses.Captured;

                bool CanCharge() =>
                    booking.PaymentMethod == PaymentMethods.BankTransfer &&
                    BookingStatusesForCharge.Contains(booking.Status);

                Task<Result> ChargeMoney()
                    => _accountPaymentProcessingService.ChargeMoney(account.Id, new ChargedMoneyData(
                            currency: account.Currency,
                            amount: amount,
                            reason: $"Charge money after booking '{booking.ReferenceCode}', which deadline already came",
                            referenceCode: booking.ReferenceCode),
                        agentContext.ToUserInfo());

                async Task<Result> StorePayment()
                {
                    var (paymentExistsForBooking, _, _, _) = await GetPayment(booking.Id);
                    if (paymentExistsForBooking)
                        return Result.Failure("Payment for current booking already exists");
                    
                    var now = _dateTimeProvider.UtcNow();
                    var info = new AccountPaymentInfo(clientIp);
                    var payment = new Payment
                    {
                        Amount = amount,
                        BookingId = booking.Id,
                        AccountNumber = account.Id.ToString(),
                        Currency = booking.Currency.ToString(),
                        Created = now,
                        Modified = now,
                        Status = PaymentStatuses.Captured,
                        Data = JsonConvert.SerializeObject(info),
                        AccountId = account.Id,
                        PaymentMethod = PaymentMethods.BankTransfer
                    };

                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    return Result.Success();
                }

                PaymentResponse CreateResult() => new PaymentResponse(string.Empty, CreditCardPaymentStatuses.Success, string.Empty);
            }


            async Task ChangePaymentStatusToCaptured()
            {
                if (booking.PaymentStatus == BookingPaymentStatuses.Captured)
                    return;

                booking.PaymentStatus = BookingPaymentStatuses.Captured;
                _context.Update(booking);
                await _context.SaveChangesAsync();
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


        public async Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount, AgentContext agent)
        {
            return await _accountPaymentProcessingService.TransferToChildAgency(payerAccountId, recipientAccountId, amount, agent);
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

            return Result.Ok(paymentEntity);
        }


        private static readonly HashSet<BookingStatusCodes> BookingStatusesForAuthorization = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.InternalProcessing
        };


        private static readonly HashSet<BookingStatusCodes> BookingStatusesForCharge = new HashSet<BookingStatusCodes>
        {
            BookingStatusCodes.InternalProcessing,
            BookingStatusCodes.Confirmed,
        };

        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccountPaymentProcessingService _accountPaymentProcessingService;
        private readonly IEntityLocker _locker;
    }
}