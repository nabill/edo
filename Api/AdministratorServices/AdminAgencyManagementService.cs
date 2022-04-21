using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    // TODO: Remove agency creation logic from this class https://github.com/happy-travel/agent-app-project/issues/812
    public class AdminAgencyManagementService : IAdminAgencyManagementService
    {
        public AdminAgencyManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
        }


        public Task<Result> DeactivateAgency(int agencyId, string reason)
            => GetAgency(agencyId)
                .Ensure(_ => !string.IsNullOrWhiteSpace(reason), "Reason must not be empty")
                .BindWithTransaction(_context, agency => ChangeActivityStatus(agency, ActivityStatus.NotActive)
                    .Tap(() => WriteAgencyDeactivationToAuditLog(agencyId, reason)));


        public Task<Result> ActivateAgency(int agencyId, string reason)
            => GetAgency(agencyId)
                .Ensure(_ => !string.IsNullOrWhiteSpace(reason), "Reason must not be empty")
                .BindWithTransaction(_context, agency => ChangeActivityStatus(agency, ActivityStatus.Active)
                    .Tap(() => WriteAgencyActivationToAuditLog(agencyId, reason)));


        public async Task<Result<AgencyInfo>> Get(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            var agencyInfo = await (
                    from agency in _context.Agencies
                    join rootAgency in _context.Agencies on agency.Ancestors.Any() ?
                        agency.Ancestors[0] :
                        agency.Id equals rootAgency.Id
                    from markupFormula in _context.DisplayMarkupFormulas
                        .Where(f => f.AgencyId == agency.Id && f.AgentId == null)
                        .DefaultIfEmpty()
                    join country in _context.Countries on agency.CountryCode equals country.Code
                    join admin in _context.Administrators on agency.AccountManagerId equals admin.Id into admn
                    from admin in admn.DefaultIfEmpty()
                    where agency.Id == agencyId
                    select agency.ToAgencyInfo(agency.ContractKind,
                        rootAgency.VerificationState,
                        rootAgency.Verified != null
                            ? rootAgency.Verified.Value.DateTime
                            : null,
                        country.Names,
                        languageCode,
                        markupFormula == null
                            ? string.Empty
                            : markupFormula.DisplayFormula,
                        admin != null ?
                            PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) :
                            null))
                .SingleOrDefaultAsync();

            return agencyInfo.Equals(default)
                ? Result.Failure<AgencyInfo>("Could not find specified agency")
                : agencyInfo;
        }


        public Task<Result<AgencyInfo>> GetRoot(int agencyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
            => GetAgency(agencyId)
                .Bind(a => Get(a.Ancestors.Any() ? a.Ancestors.First() : a.Id, languageCode));


        public IQueryable<AdminViewAgencyInfo> GetRootAgencies(string languageCode = LocalizationHelper.DefaultLanguageCode)
            => from agency in _context.Agencies
               from markupFormula in _context.DisplayMarkupFormulas
                   .Where(f => f.AgencyId == agency.Id && f.AgentId == null)
                   .DefaultIfEmpty()
               join country in _context.Countries on agency.CountryCode equals country.Code
               join admin in _context.Administrators on agency.AccountManagerId equals admin.Id into admn
               from admin in admn.DefaultIfEmpty()
               where agency.ParentId == null
               select new AdminViewAgencyInfo
               {
                   Id = agency.Id,
                   Name = agency.Name,
                   City = agency.City,
                   CountryName = country.Names.GetValueOrDefault(languageCode),
                   Created = agency.Created.DateTime,
                   VerificationState = agency.VerificationState,
                   AccountManagerName = admin != null ?
                       PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) :
                       null,
                   IsActive = agency.IsActive
               };


        public Task<List<AgencyInfo>> GetChildAgencies(int parentAgencyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
            => (
                    from agency in _context.Agencies
                    join rootAgency in _context.Agencies on agency.Ancestors[0] equals rootAgency.Id
                    from markupFormula in _context.DisplayMarkupFormulas
                        .Where(f => f.AgencyId == agency.Id && f.AgentId == null)
                        .DefaultIfEmpty()
                    join country in _context.Countries on agency.CountryCode equals country.Code
                    join admin in _context.Administrators on agency.AccountManagerId equals admin.Id into admn
                    from admin in admn.DefaultIfEmpty()
                    where agency.ParentId == parentAgencyId
                    select agency.ToAgencyInfo(agency.ContractKind,
                        rootAgency.VerificationState,
                        rootAgency.Verified != null
                            ? rootAgency.Verified.Value.DateTime
                            : null,
                        country.Names,
                        languageCode,
                        markupFormula == null
                            ? string.Empty
                            : markupFormula.DisplayFormula,
                        admin != null ?
                            PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) :
                            null))
                .ToListAsync();


        public Task<Result> ChangeActivityStatus(Agency agency, ActivityStatus status)
        {
            var convertedStatus = ConvertToDbStatus(status);
            if (convertedStatus == agency.IsActive)
                return Task.FromResult(Result.Success());

            return ChangeAgencyActivityStatus()
                .Tap(ChangeAgentsActivityStatus)
                .Tap(ChangeAgencyAccountsActivityStatus)
                .Tap(ChangeChildAgenciesActivityStatus);


            async Task<Result> ChangeAgencyActivityStatus()
            {
                agency.IsActive = convertedStatus;
                agency.Modified = _dateTimeProvider.UtcNow();

                _context.Update(agency);
                await _context.SaveChangesAsync();
                return Result.Success();
            }


            async Task ChangeAgencyAccountsActivityStatus()
            {
                var agencyAccounts = await _context.AgencyAccounts
                    .Where(ac => ac.AgencyId == agency.Id)
                    .ToListAsync();

                foreach (var account in agencyAccounts)
                    account.IsActive = convertedStatus;

                _context.UpdateRange(agencyAccounts);
                await _context.SaveChangesAsync();
            }


            async Task ChangeAgentsActivityStatus()
            {
                var agencyRelations = await _context.AgentAgencyRelations
                    .Where(ar => ar.AgencyId == agency.Id)
                    .ToListAsync();

                foreach (var relation in agencyRelations)
                    relation.IsActive = convertedStatus;

                _context.UpdateRange(agencyRelations);
                await _context.SaveChangesAsync();
            }


            async Task ChangeChildAgenciesActivityStatus()
            {
                var childAgencies = await _context.Agencies
                    .Where(a => a.ParentId == agency.Id && a.IsActive != convertedStatus)
                    .ToListAsync();

                foreach (var childAgency in childAgencies)
                    await ChangeActivityStatus(childAgency, status);
            }
        }


        public Task<Result<ContractKind>> GetContractKind(int agencyId)
            => GetRootAgency(agencyId)
                .Ensure(a => a.ContractKind.HasValue, "Agency contract kind unknown")
                .Map(a => a.ContractKind.Value);


        public async Task<Result<AgencyInfo>> Edit(int agencyId, ManagementEditAgencyRequest request, LocalityInfo localityInfo,
            string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return await Validate(request)
                .Bind(() => GetAgency(agencyId))
                .Tap(Edit)
                .Tap(AddLocalityInfo)
                .Tap(SaveChanges)
                .Bind(GetUpdatedAgencyInfo);


            Result Validate(ManagementEditAgencyRequest request)
            {
                return GenericValidator<ManagementEditAgencyRequest>.Validate(v =>
                {
                    v.RuleFor(r => r.Address).NotEmpty();
                    v.RuleFor(r => r.BillingEmail).EmailAddress().When(i => !string.IsNullOrWhiteSpace(i.BillingEmail));
                    v.RuleFor(r => r.Phone).NotEmpty();
                    v.RuleFor(r => r.LegalAddress).NotEmpty();
                    v.RuleFor(r => r.PreferredPaymentMethod).NotEmpty();
                    v.RuleFor(r => r.LocalityHtId).NotEmpty();
                }, request);
            }


            void Edit(Agency agency)
            {
                agency.Address = request.Address;
                agency.Phone = request.Phone;
                agency.Fax = request.Fax;
                agency.PostalCode = request.PostalCode;
                agency.Website = request.Website;
                agency.BillingEmail = request.BillingEmail;
                agency.VatNumber = request.VatNumber;
                agency.PreferredPaymentMethod = request.PreferredPaymentMethod;
                agency.LegalAddress = request.LegalAddress;

                agency.Modified = _dateTimeProvider.UtcNow();
            }


            void AddLocalityInfo(Agency agency)
            {
                agency.CountryCode = localityInfo.CountryIsoCode;
                agency.CountryHtId = localityInfo.CountryHtId;
                agency.City = localityInfo.LocalityName;
                agency.LocalityHtId = localityInfo.LocalityHtId;
            }


            Task SaveChanges(Agency agency)
            {
                _context.Update(agency);
                return _context.SaveChangesAsync();
            }


            Task<Result<AgencyInfo>> GetUpdatedAgencyInfo(Agency _)
                => Get(agencyId, languageCode);
        }


        public Task<Result<AgencyVerificationStates>> GetVerificationState(int agencyId)
            => GetRootAgency(agencyId)
                .Map(a => a.VerificationState);


        private async Task<Result<Agency>> GetAgency(int agencyId)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(ag => ag.Id == agencyId);
            if (agency == null)
                return Result.Failure<Agency>("Could not find agency with specified id");

            return Result.Success(agency);
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
        }


        private Task WriteAgencyDeactivationToAuditLog(int agencyId, string reason)
            => _managementAuditService.Write(ManagementEventType.AgencyDeactivation,
                new AgencyActivityStatusChangeEventData(agencyId, reason));


        private Task WriteAgencyActivationToAuditLog(int agencyId, string reason)
            => _managementAuditService.Write(ManagementEventType.AgencyActivation,
                new AgencyActivityStatusChangeEventData(agencyId, reason));


        private bool ConvertToDbStatus(ActivityStatus status) => status == ActivityStatus.Active;


        private readonly IManagementAuditService _managementAuditService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;
    }
}
