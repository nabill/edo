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
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Payments;
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
            IDateTimeProvider dateTimeProvider)
        {
            _accountPaymentProcessingService = accountPaymentProcessingService;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<bool> CanPayWithAccount(AgentContext agentInfo)
        {
            var agencyId = agentInfo.AgencyId;
            return await _context.AgencyAccounts
                .Where(a => a.AgencyId == agencyId && a.IsActive)
                .AnyAsync(a => a.Balance > 0);
        }


        public Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, AgentContext agent) 
            => GetAccountBalance(currency, agent.AgencyId);


        public async Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, int agencyId)
        {
            var accountInfo = await _context.AgencyAccounts
                .SingleOrDefaultAsync(a => a.Currency == currency && a.AgencyId == agencyId && a.IsActive);

            return accountInfo == null
                ? Result.Failure<AccountBalanceInfo>($"Payments with accounts for currency {currency} is not available for current counterparty")
                : Result.Success(new AccountBalanceInfo(accountInfo.Balance, accountInfo.Currency));
        }


        public async Task<Result> Refund(string referenceCode, MoneyAmount refundableAmount, UserInfo user, IPaymentsService paymentsService, string reason)
        {
            return await paymentsService.GetChargingAccount(referenceCode)
                .Bind(RefundMoneyToAccount)
                .Bind(ProcessPaymentResults);


            async Task<Result<Payment>> RefundMoneyToAccount(AgencyAccount account)
            {
                return await GetPayment(referenceCode)
                    .Check(Refund)
                    .Map(UpdatePaymentStatus);


                Task<Result> Refund(Payment _) 
                    => _accountPaymentProcessingService.RefundMoney(account.Id,
                        new ChargedMoneyData(
                            refundableAmount.Amount,
                            refundableAmount.Currency,
                            reason: reason,
                            referenceCode: referenceCode),
                        user);


                async Task<Payment> UpdatePaymentStatus(Payment payment)
                {
                    payment.Status = PaymentStatuses.Refunded;
                    payment.RefundedAmount = refundableAmount.Amount;
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();
                    return payment;
                }
            }
            
            
            Task<Result> ProcessPaymentResults(Payment payment) 
                => paymentsService.ProcessPaymentChanges(payment);
        }


        public async Task<Result<PaymentResponse>> Charge(string referenceCode, MoneyAmount amount, UserInfo user, IPaymentsService paymentsService)
        {
            var (_, isAccountFailure, account, accountError) = await paymentsService.GetChargingAccount(referenceCode);
            if (isAccountFailure)
                return Result.Failure<PaymentResponse>(accountError);

            return await ChargeMoney()
                .Bind(StorePayment)
                .Bind(ProcessPaymentResults)
                .Map(CreateResult);


            Task<Result> ChargeMoney()
                => _accountPaymentProcessingService.ChargeMoney(account.Id, new ChargedMoneyData(
                        currency: account.Currency,
                        amount: amount.Amount,
                        reason: $"Charge money after service '{referenceCode}'",
                        referenceCode: referenceCode),
                    user);


            async Task<Result<Payment>> StorePayment()
            {
                var (paymentExistsForBooking, _, _, _) = await GetPayment(referenceCode);
                if (paymentExistsForBooking)
                    return Result.Failure<Payment>("Payment for current booking already exists");

                var now = _dateTimeProvider.UtcNow();
                var info = new AccountPaymentInfo(string.Empty);
                var payment = new Payment
                {
                    Amount = amount.Amount,
                    BookingId = 0,
                    AccountNumber = account.Id.ToString(),
                    Currency = amount.Currency.ToString(),
                    Created = now,
                    Modified = now,
                    Status = PaymentStatuses.Captured,
                    Data = JsonConvert.SerializeObject(info),
                    AccountId = account.Id,
                    PaymentMethod = PaymentMethods.BankTransfer
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return payment;
            }


            Task<Result> ProcessPaymentResults(Payment payment) 
                => paymentsService.ProcessPaymentChanges(payment);

            
            PaymentResponse CreateResult() 
                => new(string.Empty, CreditCardPaymentStatuses.Success, string.Empty);
        }


        public async Task<Result> TransferToChildAgency(int payerAccountId, int recipientAccountId, MoneyAmount amount, AgentContext agent)
        {
            return await _accountPaymentProcessingService.TransferToChildAgency(payerAccountId, recipientAccountId, amount, agent);
        }


        private async Task<Result<Payment>> GetPayment(string referenceCode)
        {
            var paymentEntity = await _context.Payments.Where(p => p.ReferenceCode == referenceCode).FirstOrDefaultAsync();
            if (paymentEntity == default)
                return Result.Failure<Payment>(
                    $"Could not find a payment record for reference code {referenceCode}");

            return Result.Success(paymentEntity);
        }
        

        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccountPaymentProcessingService _accountPaymentProcessingService;
    }
}