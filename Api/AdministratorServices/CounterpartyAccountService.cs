using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyAccountService : ICounterpartyAccountService
    {
        public CounterpartyAccountService(EdoContext context,
            IEntityLocker locker,
            IManagementAuditService managementAuditService,
            IAccountBalanceAuditService accountBalanceAuditService,
            ICounterpartyBillingNotificationService counterpartyBillingNotificationService)
        {
            _context = context;
            _locker = locker;
            _managementAuditService = managementAuditService;
            _accountBalanceAuditService = accountBalanceAuditService;
            _counterpartyBillingNotificationService = counterpartyBillingNotificationService;
        }


        public async Task<List<CounterpartyBalanceInfo>> GetBalance(int counterpartyId, Currencies currency)
        {
            var accountsInfo = await _context.CounterpartyAccounts
                .Where(a => a.IsActive && a.Currency == currency && a.CounterpartyId == counterpartyId)
                .Select(a => new CounterpartyBalanceInfo(a.Id, a.Balance, a.Currency))
                .ToListAsync();

            return accountsInfo;
        }


        public async Task<Result> AddMoney(int counterpartyAccountId, PaymentData data, ApiCaller apiCaller)
        {
            return await GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, data), "Account and payment currency mismatch")
                .Ensure(a => IsReasonProvided(a, data), "Payment reason cannot be empty")
                .Ensure(a => IsAmountPositive(a, data), "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, account => Result.Success(account)
                        .Map(AddMoneyToCounterparty)
                        .Map(WriteAuditLog)))
                .Tap(SendNotification);


            async Task<CounterpartyAccount> AddMoneyToCounterparty(CounterpartyAccount account)
            {
                account.Balance += data.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
                => await WriteLog(account, data, AccountEventType.CounterpartyAdd, apiCaller);


            Task SendNotification(CounterpartyAccount account)
                => _counterpartyBillingNotificationService.NotifyAdded(account.CounterpartyId, data);
        }


        public async Task<Result> SubtractMoney(int counterpartyAccountId, PaymentData data, ApiCaller apiCaller)
        {
            return await GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, data), "Account and payment currency mismatch")
                .Ensure(a => IsReasonProvided(a, data), "Payment reason cannot be empty")
                .Ensure(a => IsAmountPositive(a, data), "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, account => Result.Success(account)
                        .Map(SubtractMoney)
                        .Map(WriteAuditLog)))
                .Tap(SendNotification);


            async Task<CounterpartyAccount> SubtractMoney(CounterpartyAccount account)
            {
                account.Balance -= data.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
                => await WriteLog(account, data, AccountEventType.CounterpartySubtract, apiCaller);


            Task SendNotification(CounterpartyAccount account)
                => _counterpartyBillingNotificationService.NotifySubtracted(account.CounterpartyId, data);
        }


        public async Task<Result> TransferToDefaultAgency(int counterpartyAccountId, MoneyAmount amount, ApiCaller apiCaller)
        {
            return await Result.Success(counterpartyAccountId)
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .Bind(GetCounterpartyAccount)
                .Ensure(a => AreCurrenciesMatch(a, amount), "Account and payment currency mismatch")
                .Bind(GetDefaultAgencyAccount)
                .BindWithLock(_locker, a => Result.Success(a)
                    .Ensure(IsBalanceSufficient, "Could not charge money, insufficient balance")
                    .BindWithTransaction(_context, accounts => Result.Success(accounts)
                        .Map(TransferMoney)
                        .Map(WriteAuditLog)));

            bool IsAmountPositive(int _) => amount.Amount.IsGreaterThan(decimal.Zero);


            bool IsBalanceSufficient((CounterpartyAccount counterpartyAccount, AgencyAccount agencyAccount) accounts)
                => accounts.counterpartyAccount.Balance.IsGreaterOrEqualThan(amount.Amount);


            async Task<Result<(CounterpartyAccount, AgencyAccount)>> GetDefaultAgencyAccount(CounterpartyAccount counterpartyAccount)
            {
                var defaultAgency = await _context.Agencies
                    .Where(a => a.CounterpartyId == counterpartyAccount.CounterpartyId && a.ParentId == null)
                    .SingleOrDefaultAsync();

                if (defaultAgency == null)
                    return Result.Failure<(CounterpartyAccount, AgencyAccount)>("Could not find the default agency of the account owner");

                var agencyAccount = await _context.AgencyAccounts
                    .Where(a => a.AgencyId == defaultAgency.Id && a.Currency == amount.Currency)
                    .SingleOrDefaultAsync();

                if (agencyAccount == null)
                    return Result.Failure<(CounterpartyAccount, AgencyAccount)>("Could not find the default agency account");

                return Result.Success<(CounterpartyAccount, AgencyAccount)>((counterpartyAccount, agencyAccount));
            }


            async Task<(CounterpartyAccount, AgencyAccount)> TransferMoney((CounterpartyAccount, AgencyAccount) accounts)
            {
                var (counterpartyAccount, agencyAccount) = accounts;

                counterpartyAccount.Balance -= amount.Amount;
                _context.Update(counterpartyAccount);

                agencyAccount.Balance += amount.Amount;
                _context.Update(agencyAccount);

                await _context.SaveChangesAsync();

                return (counterpartyAccount, agencyAccount);
            }


            async Task<(CounterpartyAccount, AgencyAccount)> WriteAuditLog((CounterpartyAccount, AgencyAccount) accounts)
            {
                var (counterpartyAccount, agencyAccount) = accounts;

                var counterpartyEventData = new CounterpartyAccountBalanceLogEventData(null, counterpartyAccount.Balance);
                await _accountBalanceAuditService.Write(AccountEventType.CounterpartyTransferToAgency,
                    counterpartyAccount.Id,
                    amount.Amount,
                    apiCaller,
                    counterpartyEventData,
                    null);

                var agencyEventData = new AccountBalanceLogEventData(null, agencyAccount.Balance);
                await _accountBalanceAuditService.Write(AccountEventType.CounterpartyTransferToAgency,
                    agencyAccount.Id,
                    amount.Amount,
                    apiCaller,
                    agencyEventData,
                    null);

                return (counterpartyAccount, agencyAccount);
            }
        }


        public async Task<Result> DecreaseManually(int counterpartyAccountId, PaymentData data, ApiCaller apiCaller)
        {
            return await GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, data), "Account and payment currency mismatch")
                .Ensure(a => IsReasonProvided(a, data), "Payment reason cannot be empty")
                .Ensure(a => IsAmountPositive(a, data), "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, account => Result.Success(account)
                        .Map(Decrease)
                        .Map(WriteAuditLog)))
                .Tap(SendNotification);


            async Task<CounterpartyAccount> Decrease(CounterpartyAccount account)
            {
                account.Balance -= data.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
                => await WriteLog(account, data, AccountEventType.ManualDecrease, apiCaller);


            Task SendNotification(CounterpartyAccount account)
                => _counterpartyBillingNotificationService.NotifyDecreasedManually(account.CounterpartyId, data);
        }


        public async Task<List<CounterpartyAccountInfo>> Get(int counterpartyId)
        {
            return await _context.CounterpartyAccounts
                .Where(c => c.CounterpartyId == counterpartyId)
                .Select(c => new CounterpartyAccountInfo
                {
                    Id = c.Id,
                    Currency = c.Currency,
                    Balance = new MoneyAmount
                    {
                        Amount = c.Balance,
                        Currency = c.Currency
                    },
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }


        public async Task<Result> Activate(int counterpartyId, int counterpartyAccountId, string reason)
            => await ChangeAccountActivity(counterpartyId, counterpartyAccountId, isActive: true, reason);


        public async Task<Result> Deactivate(int counterpartyId, int counterpartyAccountId, string reason)
            => await ChangeAccountActivity(counterpartyId, counterpartyAccountId, isActive: false, reason);


        public async Task<Result> IncreaseManually(int counterpartyAccountId, PaymentData data, ApiCaller apiCaller)
        {
            return await GetCounterpartyAccount(counterpartyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, data), "Account and payment currency mismatch")
                .Ensure(a => IsReasonProvided(a, data), "Payment reason cannot be empty")
                .Ensure(a => IsAmountPositive(a, data), "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, account => Result.Success(account)
                        .Map(Increase)
                        .Map(WriteAuditLog)))
                .Tap(SendNotification);


            async Task<CounterpartyAccount> Increase(CounterpartyAccount account)
            {
                account.Balance += data.Amount;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return account;
            }


            async Task<CounterpartyAccount> WriteAuditLog(CounterpartyAccount account)
                => await WriteLog(account, data, AccountEventType.ManualIncrease, apiCaller);


            Task SendNotification(CounterpartyAccount account)
                => _counterpartyBillingNotificationService.NotifyIncreasedManually(account.CounterpartyId, data);

        }


        private async Task<Result> ChangeAccountActivity(int counterpartyId, int counterpartyAccountId, bool isActive, string reason)
        {
            return await GetCounterpartyAccount(counterpartyId, counterpartyAccountId)
                .Ensure(_ => !string.IsNullOrWhiteSpace(reason), "Reason must not be empty")
                .BindWithTransaction(_context, account => SetAccountActivityState(account, isActive)
                    .Bind(() => WriteToAuditLog(counterpartyId, counterpartyAccountId, isActive, reason)));


            async Task<Result<CounterpartyAccount>> GetCounterpartyAccount(int counterpartyId, int counterpartyAccountId)
            {
                var account = await _context.CounterpartyAccounts
                    .SingleOrDefaultAsync(aa => aa.CounterpartyId == counterpartyId && aa.Id == counterpartyAccountId);

                return (account is not null)
                    ? account
                    : Result.Failure<CounterpartyAccount>($"Account Id {counterpartyAccountId} not found in counterparty Id {counterpartyId}");
            }


            async Task<Result> SetAccountActivityState(CounterpartyAccount account, bool isActive)
            {
                account.IsActive = isActive;
                _context.CounterpartyAccounts.Update(account);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            async Task<Result> WriteToAuditLog(int counterpartyId, int counterpartyAccountId, bool isActive, string reason)
            {
                if (isActive)
                    return await _managementAuditService.Write(ManagementEventType.CounterpartyAccountActivation,
                        new CounterpartyAccountActivityStatusChangeEventData(counterpartyId, counterpartyAccountId, reason));
                else
                    return await _managementAuditService.Write(ManagementEventType.CounterpartyAccountDeactivation,
                        new CounterpartyAccountActivityStatusChangeEventData(counterpartyId, counterpartyAccountId, reason));
            }
        }


        private async Task<Result<CounterpartyAccount>> GetCounterpartyAccount(int counterpartyAccountId)
        {
            var account = await _context.CounterpartyAccounts.SingleOrDefaultAsync(p => p.IsActive && p.Id == counterpartyAccountId);
            return account == default
                ? Result.Failure<CounterpartyAccount>("Could not find active counterparty account")
                : Result.Success(account);
        }


        private static bool AreCurrenciesMatch(CounterpartyAccount account, PaymentData paymentData) => account.Currency == paymentData.Currency;


        private static bool AreCurrenciesMatch(CounterpartyAccount account, MoneyAmount amount) => account.Currency == amount.Currency;


        private static bool IsReasonProvided(CounterpartyAccount account, PaymentData data) => !string.IsNullOrEmpty(data.Reason);


        private static bool IsAmountPositive(CounterpartyAccount account, PaymentData data) => data.Amount.IsGreaterThan(decimal.Zero);


        private async Task<CounterpartyAccount> WriteLog(CounterpartyAccount account, PaymentData data, AccountEventType eventType, ApiCaller apiCaller)
        {
            var eventData = new CounterpartyAccountBalanceLogEventData(data.Reason, account.Balance);
            await _accountBalanceAuditService.Write(eventType,
                account.Id,
                data.Amount,
                apiCaller,
                eventData,
                null);

            return account;
        }


        private readonly IManagementAuditService _managementAuditService;
        private readonly IAccountBalanceAuditService _accountBalanceAuditService;
        private readonly ICounterpartyBillingNotificationService _counterpartyBillingNotificationService;
        private readonly EdoContext _context;
        private readonly IEntityLocker _locker;
    }
}