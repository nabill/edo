using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class AccountPaymentProcessingService : IAccountPaymentProcessingService
    {
        public AccountPaymentProcessingService(EdoContext context,
            IEntityLocker locker,
            IAccountBalanceAuditService auditService)
        {
            _context = context;
            _locker = locker;
            _auditService = auditService;
        }


        public Task<Result> AddMoney(int accountId, PaymentData paymentData, UserInfo user)
        {
            return GetAccount(accountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AddMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool IsReasonProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            Task<Result> UnlockAccount(Result<PaymentAccount> result) => this.UnlockAccount(result, accountId);


            async Task<PaymentAccount> AddMoney(PaymentAccount account)
            {
                account.Balance += paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
            {
                var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance, account.CreditLimit, account.AuthorizedBalance);
                await _auditService.Write(AccountEventType.Add,
                    account.Id,
                    paymentData.Amount,
                    user,
                    eventData,
                    null);

                return account;
            }
        }


        public Task<Result> ChargeMoney(int accountId, PaymentData paymentData, UserInfo user)
        {
            return GetAccount(accountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsBalanceSufficient, "Could not charge money, insufficient balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(ChargeMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool IsReasonProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsBalanceSufficient(PaymentAccount account) => this.IsBalanceSufficient(account, paymentData.Amount);


            async Task<PaymentAccount> ChargeMoney(PaymentAccount account)
            {
                account.Balance -= paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
            {
                var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance, account.CreditLimit, account.AuthorizedBalance);
                await _auditService.Write(AccountEventType.Charge,
                    account.Id,
                    paymentData.Amount,
                    user,
                    eventData,
                    null);

                return account;
            }


            Task<Result> UnlockAccount(Result<PaymentAccount> result) => this.UnlockAccount(result, accountId);
        }


        public Task<Result> AuthorizeMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user)
        {
            return GetAccount(accountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsBalancePositive, "Could not charge money, insufficient balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AuthorizeMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool IsReasonProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsBalancePositive(PaymentAccount account) => account.Balance + account.CreditLimit > 0;


            async Task<PaymentAccount> AuthorizeMoney(PaymentAccount account)
            {
                account.AuthorizedBalance += paymentData.Amount;
                account.Balance -= paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
            {
                var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance, account.CreditLimit, account.AuthorizedBalance);
                await _auditService.Write(AccountEventType.Authorize,
                    account.Id,
                    paymentData.Amount,
                    user,
                    eventData,
                    paymentData.ReferenceCode);

                return account;
            }


            Task<Result> UnlockAccount(Result<PaymentAccount> result) => this.UnlockAccount(result, accountId);
        }


        public Task<Result> CaptureMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user)
        {
            return GetAccount(accountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsAuthorizedSufficient, "Could not capture money, insufficient authorized balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(CaptureMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool IsReasonProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsAuthorizedSufficient(PaymentAccount account) => this.IsAuthorizedSufficient(account, paymentData.Amount);


            async Task<PaymentAccount> CaptureMoney(PaymentAccount account)
            {
                account.AuthorizedBalance -= paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
                => WriteAuditLogWithReferenceCode(account, paymentData, AccountEventType.Capture, user);


            Task<Result> UnlockAccount(Result<PaymentAccount> result) => this.UnlockAccount(result, accountId);
        }


        public Task<Result> VoidMoney(int accountId, AuthorizedMoneyData paymentData, UserInfo user)
        {
            return GetAccount(accountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsAuthorizedSufficient, "Could not void money, insufficient authorized balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(VoidMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool IsReasonProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsAuthorizedSufficient(PaymentAccount account) => this.IsAuthorizedSufficient(account, paymentData.Amount);


            async Task<PaymentAccount> VoidMoney(PaymentAccount account)
            {
                account.AuthorizedBalance -= paymentData.Amount;
                account.Balance += paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            Task<PaymentAccount> WriteAuditLog(PaymentAccount account)
                => WriteAuditLogWithReferenceCode(account, paymentData, AccountEventType.Void, user);


            Task<Result> UnlockAccount(Result<PaymentAccount> result) => this.UnlockAccount(result, accountId);
        }


        private bool IsBalanceSufficient(PaymentAccount account, decimal amount) => account.Balance + account.CreditLimit >= amount;


        private bool IsAuthorizedSufficient(PaymentAccount account, decimal amount) => account.AuthorizedBalance >= amount;


        private bool IsCurrenciesMatch(PaymentAccount account, PaymentData paymentData) => account.Currency == paymentData.Currency;

        private bool IsCurrenciesMatch(PaymentAccount account, AuthorizedMoneyData paymentData) => account.Currency == paymentData.Currency;


        private async Task<Result<PaymentAccount>> GetAccount(int accountId)
        {
            var account = await _context.PaymentAccounts.SingleOrDefaultAsync(p => p.Id == accountId);
            return account == default
                ? Result.Failure<PaymentAccount>("Could not find account")
                : Result.Ok(account);
        }


        private async Task<Result<PaymentAccount>> LockAccount(PaymentAccount account)
        {
            var (isSuccess, _, error) = await _locker.Acquire<PaymentAccount>(account.Id.ToString(), nameof(IAccountPaymentProcessingService));
            return isSuccess
                ? Result.Ok(account)
                : Result.Failure<PaymentAccount>(error);
        }


        private async Task<PaymentAccount> WriteAuditLogWithReferenceCode(PaymentAccount account, AuthorizedMoneyData paymentData, AccountEventType eventType,
            UserInfo user)
        {
            var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance, account.CreditLimit, account.AuthorizedBalance);
            await _auditService.Write(eventType,
                account.Id,
                paymentData.Amount,
                user,
                eventData,
                paymentData.ReferenceCode);

            return account;
        }


        private async Task<Result> UnlockAccount(Result<PaymentAccount> result, int accountId)
        {
            await _locker.Release<PaymentAccount>(accountId.ToString());
            return result;
        }


        private readonly IAccountBalanceAuditService _auditService;
        private readonly EdoContext _context;
        private readonly IEntityLocker _locker;
    }
}