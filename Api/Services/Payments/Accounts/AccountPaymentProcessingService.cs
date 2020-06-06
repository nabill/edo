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
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AddMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool ReasonIsProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(PaymentAccount account) => account.Currency == paymentData.Currency;

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


        public Task<Result> AddMoneyCounterparty(int counterpartyAccountId, PaymentData paymentData, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(AmountIsPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AddMoneyCounterparty)
                    .Map(WriteAuditLog))
                .Finally(result => UnlockCounterpartyAccount(result, counterpartyAccountId));

            bool ReasonIsProvided(CounterpartyAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(CounterpartyAccount account) => account.Currency == paymentData.Currency;

            bool AmountIsPositive(CounterpartyAccount account) => paymentData.Amount > 0m;


            async Task<CounterpartyAccount> AddMoneyCounterparty(CounterpartyAccount account)
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
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(BalanceIsSufficient, "Could not charge money, insufficient balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(ChargeMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool ReasonIsProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(PaymentAccount account) => account.Currency == paymentData.Currency;

            bool BalanceIsSufficient(PaymentAccount account) => this.BalanceIsSufficient(account, paymentData.Amount);


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
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(BalanceIsPositive, "Could not charge money, insufficient balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(AuthorizeMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool ReasonIsProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(PaymentAccount account) => account.Currency == paymentData.Currency;

            bool BalanceIsPositive(PaymentAccount account) => account.Balance + account.CreditLimit > 0;


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
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(AuthorizedIsSufficient, "Could not capture money, insufficient authorized balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(CaptureMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool ReasonIsProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(PaymentAccount account) => account.Currency == paymentData.Currency;

            bool AuthorizedIsSufficient(PaymentAccount account) => this.AuthorizedIsSufficient(account, paymentData.Amount);


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
                .Ensure(ReasonIsProvided, "Payment reason cannot be empty")
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(AuthorizedIsSufficient, "Could not void money, insufficient authorized balance")
                .Bind(LockAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(VoidMoney)
                    .Map(WriteAuditLog)
                )
                .Finally(UnlockAccount);

            bool ReasonIsProvided(PaymentAccount account) => !string.IsNullOrEmpty(paymentData.Reason);

            bool CurrencyIsCorrect(PaymentAccount account) => account.Currency == paymentData.Currency;

            bool AuthorizedIsSufficient(PaymentAccount account) => this.AuthorizedIsSufficient(account, paymentData.Amount);


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


        public Task<Result> SubtractMoneyCounterparty(int counterpartyAccountId, PaymentCancellationData data, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(AmountIsPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .BindWithTransaction(_context, account => Result.Ok(account)
                    .Map(SubtractMoney)
                    .Map(WriteAuditLog))
                .Finally(result => UnlockCounterpartyAccount(result, counterpartyAccountId));

            bool CurrencyIsCorrect(CounterpartyAccount account) => account.Currency == data.Currency;

            bool AmountIsPositive(CounterpartyAccount account) => data.Amount > 0m;


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


        public Task<Result> TransferToDefaultAgency(int counterpartyAccountId, TransferData transferData, UserInfo user)
        {
            return GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(CurrencyIsCorrect, "Account and payment currency mismatch")
                .Ensure(AmountIsPositive, "Payment amount must be a positive number")
                .Bind(LockCounterpartyAccount)
                .Ensure(BalanceIsSufficient, "Could not charge money, insufficient balance")
                .Bind(GetDefaultAgencyAccount)
                .Bind(LockPaymentAccount)
                .BindWithTransaction(_context, accounts => Result.Ok(accounts)
                    .Map(TransferMoney)
                    .Map(WriteAuditLog))
                .Finally(UnlockAccounts);

            bool CurrencyIsCorrect(CounterpartyAccount account) => account.Currency == transferData.Currency;

            bool AmountIsPositive(CounterpartyAccount account) => transferData.Amount > 0m;

            bool BalanceIsSufficient(CounterpartyAccount account) => account.Balance >= transferData.Amount;


            async Task<Result<(CounterpartyAccount, PaymentAccount)>> GetDefaultAgencyAccount(CounterpartyAccount cAccount)
            {
                var defaultAgency = await _context.Agencies
                    .Where(a => a.CounterpartyId == cAccount.CounterpartyId && a.IsDefault)
                    .SingleOrDefaultAsync();

                if (defaultAgency == null)
                    return Result.Failure<(CounterpartyAccount, PaymentAccount)>("Could not find the default agency of the account owner");

                var paymentAccount = await _context.PaymentAccounts
                    .Where(a => a.AgencyId == defaultAgency.Id && a.Currency == transferData.Currency)
                    .SingleOrDefaultAsync();

                if (paymentAccount == null)
                    return Result.Failure<(CounterpartyAccount, PaymentAccount)>("Could not find the default agency payment account");

                return Result.Ok<(CounterpartyAccount, PaymentAccount)>((cAccount, paymentAccount));
            }


            async Task<Result<(CounterpartyAccount, PaymentAccount)>> LockPaymentAccount((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (cAccount, pAccount) = accounts;
                var (isSuccess, _, _, error) = await LockAccount(pAccount);
                return isSuccess 
                    ? Result.Ok<(CounterpartyAccount, PaymentAccount)>((cAccount, pAccount))
                    : Result.Failure<(CounterpartyAccount, PaymentAccount)>(error);
            }


            async Task<(CounterpartyAccount, PaymentAccount)> TransferMoney((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (cAccount, pAccount) = accounts;

                cAccount.Balance -= transferData.Amount;
                _context.Update(cAccount);

                pAccount.Balance += transferData.Amount;
                _context.Update(pAccount);

                await _context.SaveChangesAsync();
                
                return (cAccount, pAccount);
            }


            async Task<(CounterpartyAccount, PaymentAccount)> WriteAuditLog((CounterpartyAccount, PaymentAccount) accounts)
            {
                var (cAccount, pAccount) = accounts;

                var counterpartyEventData = new CounterpartyAccountBalanceLogEventData(null, cAccount.Balance);
                await _auditService.Write(AccountEventType.CounterpartyTransferToAgency,
                    cAccount.Id,
                    transferData.Amount,
                    user,
                    counterpartyEventData,
                    null);

                var agencyEventData = new AccountBalanceLogEventData(null, pAccount.Balance,
                    pAccount.CreditLimit, pAccount.AuthorizedBalance);
                await _auditService.Write(AccountEventType.Add,
                    pAccount.Id,
                    transferData.Amount,
                    user,
                    agencyEventData,
                    null);

                return (cAccount, pAccount);
            }


            async Task<Result> UnlockAccounts(Result<(CounterpartyAccount, PaymentAccount)> result)
            {
                var (isSuccess, _, (cAccount, pAccount), error) = result;

                var newResult = isSuccess ? Result.Ok() : Result.Failure(error);

                if (cAccount != default)
                    await UnlockCounterpartyAccount(Result.Ok(cAccount), cAccount.Id);

                if (pAccount != default)
                    await UnlockAccount(Result.Ok(pAccount), pAccount.Id);

                return newResult;
            }
        }


        private bool BalanceIsSufficient(PaymentAccount account, decimal amount) => account.Balance + account.CreditLimit >= amount;


        private bool AuthorizedIsSufficient(PaymentAccount account, decimal amount) => account.AuthorizedBalance >= amount;


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