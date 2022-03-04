using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
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
            IBalanceManagementNotificationsService balanceManagementNotificationsService)
        {
            _accountPaymentProcessingService = accountPaymentProcessingService;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _balanceManagementNotificationsService = balanceManagementNotificationsService;
        }


        public async Task<bool> CanPayWithAccount(AgentContext agentInfo)
        {
            var agencyId = agentInfo.AgencyId;
            return await _context.AgencyAccounts
                .Where(a => a.AgencyId == agencyId && a.IsActive)
                .AnyAsync(a => a.Balance > 0);
        }


        public Task<List<AgencyAccountInfo>> GetAgencyAccounts(AgentContext agent)
            => _context.AgencyAccounts
                .Where(a => a.AgencyId == agent.AgencyId && a.IsActive)
                .Select(a => new AgencyAccountInfo
                {
                    Balance = new MoneyAmount
                    {
                        Amount = a.Balance,
                        Currency = a.Currency
                    },
                    Currency = a.Currency,
                    Id = a.Id
                })
                .ToListAsync();


        public Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, AgentContext agent) 
            => GetAccountBalance(currency, agent.AgencyId);


        public async Task<Result<AccountBalanceInfo>> GetAccountBalance(Currencies currency, int agencyId)
        {
            var accountInfo = await _context.AgencyAccounts
                .SingleOrDefaultAsync(a => a.Currency == currency && a.AgencyId == agencyId && a.IsActive);

            return accountInfo == null
                ? Result.Failure<AccountBalanceInfo>($"Payments with accounts for currency {currency} is not available for current agency")
                : Result.Success(new AccountBalanceInfo(accountInfo.Balance, accountInfo.Currency));
        }


        public async Task<Result> Refund(string referenceCode, ApiCaller apiCaller, DateTimeOffset operationDate, IPaymentCallbackService paymentCallbackService,
            string reason)
        {
            return await GetChargingAccountId()
                .Bind(GetRefundableAmount)
                .Bind(RefundMoneyToAccount)
                .Bind(ProcessPaymentResults);

            
            Task<Result<int>> GetChargingAccountId() 
                => paymentCallbackService.GetChargingAccountId(referenceCode);
            
            
            async Task<Result<(int accountId, MoneyAmount)>> GetRefundableAmount(int accountId)
            {
                var (_, isFailure, refundableAmount, error) = await paymentCallbackService.GetRefundableAmount(referenceCode, operationDate);
                if (isFailure)
                    return Result.Failure<(int, MoneyAmount)>(error);

                return (accountId, refundableAmount);
            }

            
            async Task<Result<Payment>> RefundMoneyToAccount((int, MoneyAmount) refundInfo)
            {
                var (accountId, refundableAmount) = refundInfo;
                return await GetPayment(referenceCode)
                    .Check(Refund)
                    .Map(UpdatePaymentStatus);


                Task<Result> Refund(Payment _) 
                    => _accountPaymentProcessingService.RefundMoney(accountId,
                        new ChargedMoneyData(
                            refundableAmount.Amount,
                            refundableAmount.Currency,
                            reason: reason,
                            referenceCode: referenceCode),
                        apiCaller);


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
                => paymentCallbackService.ProcessPaymentChanges(payment);
        }


        public async Task<Result<PaymentResponse>> Charge(string referenceCode, ApiCaller apiCaller, IPaymentCallbackService paymentCallbackService)
        {
            return await GetChargingAccountId()
                .Bind(GetChargingAccount)
                .Bind(GetChargingAmount)
                .Check(ChargeMoney)
                .Tap(SendNotificationIfRequired)
                .Bind(StorePayment)
                .Bind(ProcessPaymentResults)
                .Map(CreateResult);

            
            Task<Result<int>> GetChargingAccountId() 
                => paymentCallbackService.GetChargingAccountId(referenceCode);


            async Task<Result<AgencyAccount>> GetChargingAccount(int accountId)
                => await _context.AgencyAccounts.SingleOrDefaultAsync(a => a.Id == accountId) 
                    ?? Result.Failure<AgencyAccount>("Could not find agency account");


            async Task<Result<(AgencyAccount, MoneyAmount)>> GetChargingAmount(AgencyAccount account)
            {
                var (_, isFailure, amount, error) = await paymentCallbackService.GetChargingAmount(referenceCode);
                if (isFailure)
                    return Result.Failure<(AgencyAccount, MoneyAmount)>(error);

                return (account, amount);
            }
            
            
            Task<Result> ChargeMoney((AgencyAccount account, MoneyAmount amount) chargeInfo)
            {
                var (account, amount) = chargeInfo;
                return _accountPaymentProcessingService.ChargeMoney(account.Id, new ChargedMoneyData(
                        currency: amount.Currency,
                        amount: amount.Amount,
                        reason: $"Charge money for service '{referenceCode}'",
                        referenceCode: referenceCode),
                    apiCaller);
            }


            async Task<Result<Payment>> StorePayment((AgencyAccount account, MoneyAmount amount) chargeInfo)
            {
                var (account, amount) = chargeInfo;
                var (paymentExistsForBooking, _, _, _) = await GetPayment(referenceCode);
                if (paymentExistsForBooking)
                    return Result.Failure<Payment>("Payment for current booking already exists");

                var now = _dateTimeProvider.UtcNow();
                var info = new AccountPaymentInfo(string.Empty);
                var payment = new Payment
                {
                    Amount = amount.Amount,
                    AccountNumber = account.Id.ToString(),
                    Currency = amount.Currency,
                    Created = now,
                    Modified = now,
                    Status = PaymentStatuses.Captured,
                    Data = JsonConvert.SerializeObject(info),
                    AccountId = account.Id,
                    PaymentMethod = PaymentTypes.VirtualAccount,
                    ReferenceCode = referenceCode
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                _context.Detach(payment);

                return payment;
            }


            Task<Result> ProcessPaymentResults(Payment payment) 
                => paymentCallbackService.ProcessPaymentChanges(payment);


            Task SendNotificationIfRequired((AgencyAccount account, MoneyAmount amount) chargeInfo)
                => _balanceManagementNotificationsService.SendNotificationIfRequired(chargeInfo.account, chargeInfo.amount);
            
            
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
        private readonly IBalanceManagementNotificationsService _balanceManagementNotificationsService;
        private readonly IAccountPaymentProcessingService _accountPaymentProcessingService;
    }
}