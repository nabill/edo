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


        public Task<Result> AddMoneyToCounterparty(int counterpartyAccountId, PaymentData paymentData, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(a => IsCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AddMoneyToCounterparty)
                    .Map(WriteAuditLog))
                .Finally(result => UnlockCounterpartyAccount(result, counterpartyAccountId));

            bool IsReasonProvided(CounterpartyAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsAmountPositive(CounterpartyAccount account) => paymentData.Amount > 0m;


            async Task<CounterpartyAccount> AddMoneyToCounterparty(CounterpartyAccount account)
            {
                account.Balance += paymentData.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
            {
                var eventData = new CounterpartyAccountBalanceLogEventData(paymentData.Reason, account.Balance);
                await _auditService.Write(AccountEventType.CounterpartyAdd,
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


        public Task<Result> SubtractMoneyFromCounterparty(int counterpartyAccountId, PaymentCancellationData data, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => IsCurrenciesMatch(a, data), "Account and payment currency mismatch")
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(SubtractMoney)
                    .Map(WriteAuditLog))
                .Finally(result => UnlockCounterpartyAccount(result, counterpartyAccountId));

            bool IsAmountPositive(CounterpartyAccount account) => data.Amount > 0m;


            async Task<CounterpartyAccount> SubtractMoney(CounterpartyAccount account)
            {
                account.Balance -= data.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
            {
                var eventData = new CounterpartyAccountBalanceLogEventData(null, account.Balance);
                await _auditService.Write(AccountEventType.CounterpartySubtract,
                    account.Id,
                    data.Amount,
                    user,
                    eventData,
                    null);

                return account;
            }
        }


        public Task<Result> TransferToDefaultAgency(int counterpartyAccountId, MoneyAmount amount, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => IsCurrenciesMatch(a, amount), "Account and payment currency mismatch")
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .Ensure(IsBalanceSufficient, "Could not charge money, insufficient balance")
                .Bind(GetDefaultAgencyAccount)
                .Bind(LockPaymentAccount)
                .BindWithTransaction(_context, accounts => Result.Ok(accounts)
                    .Map(TransferMoney)
                    .Map(WriteAuditLog))
                .Finally(UnlockAccounts);

            bool IsAmountPositive(CounterpartyAccount account) => amount.Amount > 0m;

            bool IsBalanceSufficient(CounterpartyAccount account) => account.Balance >= amount.Amount;


            async Task<Result<(CounterpartyAccount, PaymentAccount)>> GetDefaultAgencyAccount(CounterpartyAccount counterpartyAccount)
            {
                var defaultAgency = await _context.Agencies
                    .Where(a => a.CounterpartyId == counterpartyAccount.CounterpartyId && a.IsDefault)
                    .SingleOrDefaultAsync();

                if (defaultAgency == null)
                    return Result.Failure<(CounterpartyAccount, PaymentAccount)>("Could not find the default agency of the account owner");

                var paymentAccount = await _context.PaymentAccounts
                    .Where(a => a.AgencyId == defaultAgency.Id && a.Currency == amount.Currency)
                    .SingleOrDefaultAsync();

                if (paymentAccount == null)
                    return Result.Failure<(CounterpartyAccount, PaymentAccount)>("Could not find the default agency payment account");

                return Result.Ok<(CounterpartyAccount, PaymentAccount)>((counterpartyAccount, paymentAccount));
            }


            async Task<Result<(CounterpartyAccount, PaymentAccount)>> LockPaymentAccount((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (counterpartyAccount, paymentAccount) = accounts;
                var (isSuccess, _, _, error) = await LockAccount(paymentAccount);
                return isSuccess 
                    ? Result.Ok<(CounterpartyAccount, PaymentAccount)>((counterpartyAccount, paymentAccount))
                    : Result.Failure<(CounterpartyAccount, PaymentAccount)>(error);
            }


            async Task<(CounterpartyAccount, PaymentAccount)> TransferMoney((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (counterpartyAccount, paymentAccount) = accounts;

                counterpartyAccount.Balance -= amount.Amount;
                _context.Update(counterpartyAccount);

                paymentAccount.Balance += amount.Amount;
                _context.Update(paymentAccount);

                await _context.SaveChangesAsync();
                
                return (counterpartyAccount, paymentAccount);
            }


            async Task<(CounterpartyAccount, PaymentAccount)> WriteAuditLog((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (counterpartyAccount, paymentAccount) = accounts;

                var counterpartyEventData = new CounterpartyAccountBalanceLogEventData(null, counterpartyAccount.Balance);
                await _auditService.Write(AccountEventType.CounterpartyTransferToAgency,
                    counterpartyAccount.Id,
                    amount.Amount,
                    user,
                    counterpartyEventData,
                    null);

                var agencyEventData = new AccountBalanceLogEventData(null, paymentAccount.Balance,
                    paymentAccount.CreditLimit, paymentAccount.AuthorizedBalance);
                await _auditService.Write(AccountEventType.Add,
                    paymentAccount.Id,
                    amount.Amount,
                    user,
                    agencyEventData,
                    null);

                return (counterpartyAccount, paymentAccount);
            }


            async Task<Result> UnlockAccounts(Result<(CounterpartyAccount, PaymentAccount)> result)
            {
                var (isSuccess, _, (counterpartyAccount, paymentAccount), error) = result;

                var newResult = isSuccess ? Result.Ok() : Result.Failure(error);

                if (counterpartyAccount != default)
                    await UnlockCounterpartyAccount(Result.Ok(counterpartyAccount), counterpartyAccount.Id);

                if (paymentAccount != default)
                    await UnlockAccount(Result.Ok(paymentAccount), paymentAccount.Id);

                return newResult;
            }
        }


        private bool IsBalanceSufficient(PaymentAccount account, decimal amount) => account.Balance + account.CreditLimit >= amount;


        private bool IsAuthorizedSufficient(PaymentAccount account, decimal amount) => account.AuthorizedBalance >= amount;


        private bool IsCurrenciesMatch(PaymentAccount account, PaymentData paymentData) => account.Currency == paymentData.Currency;

        private bool IsCurrenciesMatch(PaymentAccount account, AuthorizedMoneyData paymentData) => account.Currency == paymentData.Currency;

        private bool IsCurrenciesMatch(CounterpartyAccount account, PaymentData paymentData) => account.Currency == paymentData.Currency;

        private bool IsCurrenciesMatch(CounterpartyAccount account, MoneyAmount amount) => account.Currency == amount.Currency;

        private bool IsCurrenciesMatch(CounterpartyAccount account, PaymentCancellationData data) => account.Currency == data.Currency;


        private async Task<Result<PaymentAccount>> GetAccount(int accountId)
        {
            var account = await _context.PaymentAccounts.SingleOrDefaultAsync(p => p.Id == accountId);
            return account == default
                ? Result.Failure<PaymentAccount>("Could not find account")
                : Result.Ok(account);
        }


        private async Task<Result<CounterpartyAccount>> GetCounterpartyAccount(int counterpartyAccountId)
        {
            var account = await _context.CounterpartyAccounts.SingleOrDefaultAsync(p => p.Id == counterpartyAccountId);
            return account == default
                ? Result.Failure<CounterpartyAccount>("Could not find account")
                : Result.Ok(account);
        }


        private async Task<Result<PaymentAccount>> LockAccount(PaymentAccount account)
        {
            var (isSuccess, _, error) = await _locker.Acquire<PaymentAccount>(account.Id.ToString(), nameof(IAccountPaymentProcessingService));
            return isSuccess
                ? Result.Ok(account)
                : Result.Failure<PaymentAccount>(error);
        }


        private async Task<Result<CounterpartyAccount>> LockCounterpartyAccount(CounterpartyAccount account)
        {
            var (isSuccess, _, error) = await _locker.Acquire<CounterpartyAccount>(account.Id.ToString(), nameof(IAccountPaymentProcessingService));
            return isSuccess
                ? Result.Ok(account)
                : Result.Failure<CounterpartyAccount>(error);
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


        private async Task<Result> UnlockCounterpartyAccount(Result<CounterpartyAccount> result, int accountId)
        {
            await _locker.Release<CounterpartyAccount>(accountId.ToString());
            return result;
        }


        private readonly IAccountBalanceAuditService _auditService;
        private readonly EdoContext _context;
        private readonly IEntityLocker _locker;
    }
}