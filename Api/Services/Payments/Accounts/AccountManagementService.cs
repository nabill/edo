using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class AccountManagementService : IAccountManagementService
    {
        public AccountManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            ILogger<AccountManagementService> logger,
            IAdministratorContext administratorContext,
            IManagementAuditService managementAuditService,
            IAdminAgencyManagementService adminAgencyManagementService,
            IEntityLocker locker)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _administratorContext = administratorContext;
            _managementAuditService = managementAuditService;
            _adminAgencyManagementService = adminAgencyManagementService;
            _locker = locker;
        }


        public async Task<Result> CreateForAgency(Agency agency, Currencies currency)
        {
            return await CheckAgencyVerified()
                .Map(CreateAccount)
                .Tap(LogSuccess)
                .OnFailure(LogFailure);


            async Task<Result> CheckAgencyVerified()
            {
                var (_, isFailure, verificationState, error) = await _adminAgencyManagementService.GetVerificationState(agency.Id);
                if (isFailure)
                    return Result.Failure(error);

                return new[] {AgencyVerificationStates.ReadOnly, AgencyVerificationStates.FullAccess}.Contains(verificationState)
                    ? Result.Success()
                    : Result.Failure("Account creation is only available for verified agencies");
            }


            async Task<AgencyAccount> CreateAccount()
            {
                var account = new AgencyAccount
                {
                    Balance = 0,
                    AgencyId = agency.Id,
                    Currency = Currencies.USD, // Only USD currency is supported
                    Created = _dateTimeProvider.UtcNow()
                };
                _context.AgencyAccounts.Add(account);
                _context.AgencyMarkupBonusesAccounts.Add(new AgencyMarkupBonusesAccount
                {
                    AgencyId = agency.Id,
                    Currency = Currencies.USD,
                    Balance = 0
                });
                await _context.SaveChangesAsync();

                return account;
            }


            void LogSuccess(AgencyAccount account)
            {
                _logger.LogAgencyAccountCreationSuccess(agency.Id, account.Id);
            }


            void LogFailure(string error)
            {
                _logger.LogAgencyAccountCreationFailed(agency.Id, error);
            }
        }


        public async Task<Result<AgencyAccount>> Get(int agencyId, Currencies currency)
        {
            var account = await _context.AgencyAccounts.FirstOrDefaultAsync(a => a.IsActive && a.AgencyId == agencyId && a.Currency == currency);
            return account == null
                ? Result.Failure<AgencyAccount>($"Cannot find account for agency '{agencyId}' and currency '{currency}'")
                : Result.Success(account);
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEntityLocker _locker;
        private readonly ILogger<AccountManagementService> _logger;
        private readonly IManagementAuditService _managementAuditService;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
    }
}