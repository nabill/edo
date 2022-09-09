using System.Linq;
using System.Threading.Tasks;
using Api.AdministratorServices;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
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
            ICompanyInfoService companyInfoService,
            IEntityLocker locker)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _administratorContext = administratorContext;
            _managementAuditService = managementAuditService;
            _locker = locker;
            _companyInfoService = companyInfoService;
        }


        public async Task<Result> CreateForAgency(Agency agency, Currencies currency)
        {
            return await Validate()
                .Map(CreateAccount)
                .Tap(LogSuccess)
                .OnFailure(LogFailure);


            async Task<Result> Validate()
            {
                var (_, isFailure, verificationState, error) = await GetVerificationState(agency.Id);
                if (isFailure)
                    return Result.Failure(error);

                var (_, isAccountFailure, existingAccount, accountError) = await Get(agency.Id, currency);
                if (existingAccount != default)
                    return Result.Failure($"Account have been already created with currency {currency} for agency {agency.Id}");


                return new[] { AgencyVerificationStates.ReadOnly, AgencyVerificationStates.FullAccess }.Contains(verificationState)
                    ? Result.Success()
                    : Result.Failure("Account creation is only available for verified agencies");
            }


            async Task<AgencyAccount> CreateAccount()
            {
                var account = new AgencyAccount
                {
                    Balance = 0,
                    AgencyId = agency.Id,
                    Currency = currency,
                    Created = _dateTimeProvider.UtcNow()
                };
                _context.AgencyAccounts.Add(account);
                _context.AgencyMarkupBonusesAccounts.Add(new AgencyMarkupBonusesAccount
                {
                    AgencyId = agency.Id,
                    Currency = currency,
                    Balance = 0
                });
                await _context.SaveChangesAsync();

                return account;
            }


            Task<Result<AgencyVerificationStates>> GetVerificationState(int agencyId)
            => GetRootAgency(agencyId)
                .Map(a => a.VerificationState);


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


        private Task<Result<Agency>> GetRootAgency(int agencyId)
        {
            return GetAgency(agencyId)
                .Map(GetRootAgency);


            Task<Agency> GetRootAgency(Agency currentAgency)
            {
                var rootAgencyId = currentAgency.Ancestors.Any()
                    ? currentAgency.Ancestors.First()
                    : currentAgency.Id;
                return _context.Agencies.SingleAsync(ra => ra.Id == rootAgencyId);
            }


            async Task<Result<Agency>> GetAgency(int agencyId)
            {
                var agency = await _context.Agencies.FirstOrDefaultAsync(ag => ag.Id == agencyId);
                if (agency == null)
                    return Result.Failure<Agency>("Could not find agency with specified id");

                return Result.Success(agency);
            }
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEntityLocker _locker;
        private readonly ILogger<AccountManagementService> _logger;
        private readonly IManagementAuditService _managementAuditService;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly ICompanyInfoService _companyInfoService;
    }
}