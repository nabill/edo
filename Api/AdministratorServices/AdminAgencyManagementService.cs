using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.MultiLanguage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    // TODO: Remove agency creation logic from this class https://github.com/happy-travel/agent-app-project/issues/812
    public class AdminAgencyManagementService : IAdminAgencyManagementService
    {
        public AdminAgencyManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService,
            IOptions<NakijinDbOptions> nakijinDbOptions)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
            _nakijinDbOptions = nakijinDbOptions.Value;
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
                        languageCode,
                        markupFormula == null
                            ? string.Empty
                            : markupFormula.DisplayFormula,
                        country.Names,
                        admin != null ?
                            PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) :
                            null,
                        admin != null ?
                            admin.Id :
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
            => (from agency in _context.Agencies
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
                    CountryName = country.Names.En,
                    Created = agency.Created.DateTime,
                    VerificationState = agency.VerificationState,
                    AccountManagerId = admin != null ? admin.Id : null,
                    AccountManagerName = admin != null ? PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) : null,
                    IsActive = agency.IsActive,
                    CreditLimit = agency.CreditLimit
                });


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
                        languageCode,
                        markupFormula == null
                            ? string.Empty
                            : markupFormula.DisplayFormula,
                        country.Names,
                        admin != null ?
                            PersonNameFormatters.ToMaskedName(admin.FirstName, admin.LastName, null) :
                            null,
                        admin != null ?
                            admin.Id :
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


        public async Task<Result> ChangeContractKind(int agencyId, ContractKindChangeRequest request)
        {
            return await ValidateChange(request)
                .Bind(() => GetAgency(agencyId))
                .Ensure(a => a.VerificationState == AgencyVerificationStates.FullAccess, "Agency is not fully verified")
                .Tap(SetContractKind)
                .Tap(SetAprModeAndPassedDeadlineOffersMode)
                .Tap(() => WriteChangeContractKindToAuditLog(agencyId, request.ContractKind, request.Reason));


            Task<Result> ValidateChange(ContractKindChangeRequest request)
                => GenericValidator<ContractKindChangeRequest>.ValidateAsync(v =>
                    {
                        v.RuleFor(r => r.CreditLimit)
                            .NotEmpty()
                            .When(r => r.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments);

                        v.RuleFor(r => r.CreditLimit)
                            .Empty()
                            .When(r => r.ContractKind != ContractKind.VirtualAccountOrCreditCardPayments);

                        v.RuleFor(r => r.ContractKind)
                            .NotEmpty();

                        v.RuleFor(r => r.Reason)
                            .NotEmpty();
                    }, request);


            async Task SetContractKind(Agency agency)
            {
                agency.ContractKind = request.ContractKind;
                agency.CreditLimit = (request.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments)
                    ? request.CreditLimit
                    : null;
                _context.Update(agency);
                await _context.SaveChangesAsync();
            }


            async Task SetAprModeAndPassedDeadlineOffersMode(Agency agency)
            {
                if (request.ContractKind is not (ContractKind.VirtualAccountOrCreditCardPayments or ContractKind.OfflineOrCreditCardPayments))
                    return;

                var settings = await _context.AgencySystemSettings.SingleOrDefaultAsync(a => a.AgencyId == agencyId);
                if (settings == default)
                {
                    settings = new AgencySystemSettings
                    {
                        AgencyId = agencyId
                    };
                    _context.Add(settings);
                    await _context.SaveChangesAsync();
                }

                settings.AccommodationBookingSettings ??= new AgencyAccommodationBookingSettings();

                if (request.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments)
                {
                    settings.AccommodationBookingSettings.AprMode = AprMode.CardAndAccountPurchases;
                    settings.AccommodationBookingSettings.PassedDeadlineOffersMode = PassedDeadlineOffersMode.CardAndAccountPurchases;
                }

                if (request.ContractKind == ContractKind.OfflineOrCreditCardPayments)
                {
                    settings.AccommodationBookingSettings.AprMode = AprMode.Hide;
                    settings.AccommodationBookingSettings.PassedDeadlineOffersMode = PassedDeadlineOffersMode.Hide;
                }

                _context.Update(settings);
                await _context.SaveChangesAsync();
            }
        }


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
                agency.TaxRegistrationNumber = request.TaxRegistrationNumber;

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


        public async Task<Result> FulfillLocality()
        {
            var queryString = "SELECT l.\"Id\" as \"LocalityId\", l.\"Names\" as \"LocalityNames\", c.\"Id\" as \"CountryId\"" +
                " FROM \"Localities\" l" +
                " JOIN \"Countries\" c ON c.\"Id\" = l.\"CountryId\";";
            var connectionString = $"Server={_nakijinDbOptions.Host};Port={_nakijinDbOptions.Port};" +
                $"Userid={_nakijinDbOptions.UserId};Password={_nakijinDbOptions.Password};" +
                "Database=nakijin";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                var command = new NpgsqlCommand(queryString, connection);
                await connection.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                try
                {
                    var agencies = await _context.Agencies.Where(a => a.LocalityHtId == null && a.City != null).ToListAsync();

                    while (await reader.ReadAsync())
                    {
                        var localityId = $"Locality_{reader["LocalityId"]}";
                        var localityNames = JsonConvert.DeserializeObject<MultiLanguage<string?>>(reader["LocalityNames"].ToString()!);
                        var countryId = $"Country_{reader["CountryId"]}";

                        var agency = agencies
                            .Where(a => (a.City.ToLower().Equals(localityNames!.En?.ToLower()) ||
                                a.City.ToLower().Equals(localityNames!.Ru?.ToLower())))
                            .FirstOrDefault();

                        if (agency != default)
                        {
                            agency.CountryHtId = countryId;
                            agency.LocalityHtId = localityId;

                            _context.Update(agency);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                finally
                {
                    await reader.CloseAsync();
                }
            }
            return Result.Success();
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

        private Task WriteChangeContractKindToAuditLog(int agencyId, ContractKind contractKind, string reason)
            => _managementAuditService.Write(ManagementEventType.ChangeAgencyContractKind,
                new ChangeContractKindData(agencyId, contractKind, reason));

        private bool ConvertToDbStatus(ActivityStatus status) => status == ActivityStatus.Active;


        private readonly IManagementAuditService _managementAuditService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;
        private readonly NakijinDbOptions _nakijinDbOptions;
    }
}
