using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgencyAccountService : IAgencyAccountService
    {
        public AgencyAccountService(EdoContext context, IEntityLocker locker, IManagementAuditService managementAuditService, 
            IAccountBalanceAuditService accountBalanceAuditService)
        {
            _context = context;
            _locker = locker;
            _managementAuditService = managementAuditService;
            _accountBalanceAuditService = accountBalanceAuditService;
        }


        public async Task<List<FullAgencyAccountInfo>> Get(int agencyId)
            => await _context.AgencyAccounts
                .Where(a => a.AgencyId == agencyId)
                .Select(a => new FullAgencyAccountInfo
                {
                    Id = a.Id,
                    Balance = new MoneyAmount
                    {
                        Amount = a.Balance,
                        Currency = a.Currency
                    },
                    Currency = a.Currency,
                    Created = a.Created.DateTime,
                    IsActive = a.IsActive
                })
                .ToListAsync();


        public Task<List<FullAgencyAccountInfo>> Get(int agencyId, Currencies currency)
        {
            return _context.AgencyAccounts
                .Where(a => a.AgencyId == agencyId && a.Currency == currency)
                .Select(a => new FullAgencyAccountInfo
                {
                    Id = a.Id,
                    Balance = new MoneyAmount
                    {
                        Amount = a.Balance,
                        Currency = a.Currency
                    },
                    Currency = a.Currency,
                    Created = a.Created.DateTime,
                    IsActive = a.IsActive
                })
                .ToListAsync();
        }


        public Task<List<AccountBalanceAuditLogEntry>> GetAccountHistory(int accountId)
        {
            return _context.AccountBalanceAuditLogs
                .Where(l => l.AccountId == accountId)
                .OrderBy(l => l.Created)
                .ToListAsync();
        }


        public async Task<Result> Activate(int agencyId, int agencyAccountId, string reason)
            => await ChangeAccountActivity(agencyId, agencyAccountId, isActive: true, reason);


        public async Task<Result> Deactivate(int agencyId, int agencyAccountId, string reason)
            => await ChangeAccountActivity(agencyId, agencyAccountId, isActive: false, reason);


        public Task<Result> IncreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller) 
            => IncreaseBalance(agencyAccountId, paymentData, apiCaller, AccountEventType.ManualIncrease);


        public Task<Result> DecreaseManually(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller)
            => DecreaseBalance(agencyAccountId, paymentData, apiCaller, AccountEventType.ManualDecrease);


        public Task<Result> AddMoney(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller)
            => IncreaseBalance(agencyAccountId, paymentData, apiCaller, AccountEventType.AgencyAdd);


        public Task<Result> Subtract(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller)
            => DecreaseBalance(agencyAccountId, paymentData, apiCaller, AccountEventType.AgencySubtract);

        
        private async Task<Result> IncreaseBalance(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller, AccountEventType eventType)
        {
            return await GetAgencyAccount(agencyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, accounts => Result.Success(accounts)
                        .Map(Increase)
                        .Map(WriteAuditLog)));

            bool IsReasonProvided(AgencyAccount _) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsAmountPositive(AgencyAccount _) => paymentData.Amount.IsGreaterThan(decimal.Zero);


            async Task<AgencyAccount> WriteAuditLog(AgencyAccount account)
            {
                var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance);
                await _accountBalanceAuditService.Write(eventType: eventType,
                    accountId: account.Id,
                    amount: paymentData.Amount,
                    apiCaller: apiCaller,
                    eventData: eventData,
                    null);

                return account;
            }


            async Task<AgencyAccount> Increase(AgencyAccount agencyAccount)
            {
                agencyAccount.Balance += paymentData.Amount;
                _context.Update(agencyAccount);
                await _context.SaveChangesAsync();
                return agencyAccount;
            }
        }


        private async Task<Result> DecreaseBalance(int agencyAccountId, PaymentData paymentData, ApiCaller apiCaller, AccountEventType eventType)
        {
            return await GetAgencyAccount(agencyAccountId)
                .Ensure(a => AreCurrenciesMatch(a, paymentData), "Account and payment currency mismatch")
                .Ensure(IsReasonProvided, "Payment reason cannot be empty")
                .Ensure(IsAmountPositive, "Payment amount must be a positive number")
                .BindWithLock(_locker, a => Result.Success(a)
                    .BindWithTransaction(_context, accounts => Result.Success(accounts)
                        .Map(Decrease)
                        .Map(WriteAuditLog)));

            bool IsReasonProvided(AgencyAccount _) => !string.IsNullOrEmpty(paymentData.Reason);

            bool IsAmountPositive(AgencyAccount _) => paymentData.Amount.IsGreaterThan(decimal.Zero);


            async Task<AgencyAccount> WriteAuditLog(AgencyAccount account)
            {
                var eventData = new AccountBalanceLogEventData(paymentData.Reason, account.Balance);
                await _accountBalanceAuditService.Write(eventType,
                    account.Id,
                    paymentData.Amount,
                    apiCaller,
                    eventData,
                    null);

                return account;
            }


            async Task<AgencyAccount> Decrease(AgencyAccount agencyAccount)
            {
                agencyAccount.Balance -= paymentData.Amount;
                _context.Update(agencyAccount);
                await _context.SaveChangesAsync();
                return agencyAccount;
            }
        }
        

        private async Task<Result> ChangeAccountActivity(int agencyId, int agencyAccountId, bool isActive, string reason)
        {
            return await GetAgencyAccount(agencyId, agencyAccountId)
                .Ensure(_ => !string.IsNullOrWhiteSpace(reason), "Reason must not be empty")
                .BindWithTransaction(_context, account => SetAccountActivityState(account, isActive)
                    .Bind(() => WriteToAuditLog(agencyId, agencyAccountId, isActive, reason)));


            async Task<Result<AgencyAccount>> GetAgencyAccount(int agencyId, int agencyAccountId)
            {
                var account = await _context.AgencyAccounts
                    .SingleOrDefaultAsync(aa => aa.AgencyId == agencyId && aa.Id == agencyAccountId);

                return (account is not null)
                    ? account
                    : Result.Failure<AgencyAccount>($"Account Id {agencyAccountId} not found in agency Id {agencyId}");
            }


            async Task<Result> SetAccountActivityState(AgencyAccount account, bool isActive)
            {
                account.IsActive = isActive;
                _context.AgencyAccounts.Update(account);
                await _context.SaveChangesAsync();

                return Result.Success();
            }


            async Task<Result> WriteToAuditLog(int agencyId, int agencyAccountId, bool isActive, string reason)
            {
                if (isActive)
                    return await _managementAuditService.Write(ManagementEventType.AgencyAccountActivation, 
                        new AgencyAccountActivityStatusChangeEventData(agencyId, agencyAccountId, reason));
                else
                    return await _managementAuditService.Write(ManagementEventType.AgencyAccountDeactivation, 
                        new AgencyAccountActivityStatusChangeEventData(agencyId, agencyAccountId, reason));
            }
        }


        private async Task<Result<AgencyAccount>> GetAgencyAccount(int agencyAccountId)
        {
            var agencyAccount = await _context.AgencyAccounts
                .SingleOrDefaultAsync(ac => ac.Id == agencyAccountId);

            if (agencyAccount == default)
                return Result.Failure<AgencyAccount>("Could not found an account.");

            return Result.Success(agencyAccount);
        }


        private bool AreCurrenciesMatch(AgencyAccount account, PaymentData paymentData) => account.Currency == paymentData.Currency;


        private readonly IManagementAuditService _managementAuditService; 
        private readonly IAccountBalanceAuditService _accountBalanceAuditService;
        private readonly IEntityLocker _locker;
        private readonly EdoContext _context;
    }
}