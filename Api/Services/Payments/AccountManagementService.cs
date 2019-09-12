using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class AccountManagementService : IAccountManagementService
    {
        public AccountManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ILogger<AccountManagementService> logger,
            IAdministratorContext administratorContext)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _administratorContext = administratorContext;
        }
        
        public async Task<Result>CreateAccount(Company company, Currencies currency)
        {
            return await Result.Ok()
                .Ensure(CompanyIsVerified, "Account creation is only available for verified companies")
                .OnSuccess(CreateAccount)
                .OnSuccess(LogSuccess)
                .OnFailure(LogFailure);
            
            bool CompanyIsVerified()
            {
                return company.State == CompanyStates.Verified;
            }

            async Task<PaymentAccount> CreateAccount()
            {
                var account = new PaymentAccount
                {
                    Balance = 0,
                    CreditLimit = 0,
                    CompanyId = company.Id,
                    Currency = currency,
                    Created = _dateTimeProvider.UtcNow()
                };
                _context.PaymentAccounts.Add(account);
                await _context.SaveChangesAsync();
                return account;
            }

            void LogSuccess(PaymentAccount account)
            {
                _logger.LogPaymentAccountCreationSuccess(
                    $"Successfully created payment account for company: '{company.Id}', account id: {account.Id}");
            }

            void LogFailure(string error)
            {
                _logger.LogPaymentAccountCreationFailed($"Failed to create account for company {company.Id}, error {error}");
            }
        }

        public Task<Result> ChangeCreditLimit(int accountId, decimal creditLimit)
        {
            return GetAdminContext()
                .Ensure(CreditLimitIsValid, "Credit limit should be greater than zero")
                .OnSuccess(UpdateCreditLimit);
                
            // TODO logs + audit.
            
            async Task<Result> GetAdminContext()
            {
                return (await _administratorContext.HasGlobalPermission(GlobalPermissions.CreditLimitChange)
                    ? Result.Ok()
                    : Result.Fail("No rights to change credit limit"));
            }
            
            bool CreditLimitIsValid()
            {
                return creditLimit >= 0;
            }

            async Task<Result> UpdateCreditLimit()
            {
                var account = await _context.PaymentAccounts.SingleOrDefaultAsync(p => p.Id == accountId);
                if (account == default)
                    return Result.Fail("Could not find payment account");

                account.CreditLimit = creditLimit;
                _context.Update(account);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }

        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AccountManagementService> _logger;
        private readonly IAdministratorContext _administratorContext;
    }
}