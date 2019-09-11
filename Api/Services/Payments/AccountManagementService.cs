using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Payments;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class AccountManagementService : IAccountManagementService
    {
        public AccountManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ILogger<AccountManagementService> logger)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
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
            throw new System.NotImplementedException();
        }
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<AccountManagementService> _logger;
    }
}