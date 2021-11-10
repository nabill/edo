﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    // TODO: Remove agency creation logic from this class https://github.com/happy-travel/agent-app-project/issues/812
    public class AdminAgencyManagementService : IAdminAgencyManagementService
    {
        public AdminAgencyManagementService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService,
            ILocalityInfoService localityInfoService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
            _localityInfoService = localityInfoService;
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
                    from a in _context.Agencies
                    join c in _context.Countries on a.CountryCode equals c.Code
                    join cp in _context.Counterparties on a.CounterpartyId equals cp.Id
                    join ra in _context.Agencies on a.Ancestors.Any() ? a.Ancestors[0] : a.Id equals ra.Id
                    where a.Id == agencyId
                    select a.ToAgencyInfo(a.ContractKind, ra.VerificationState, ra.Verified, c.Names, languageCode))
                .SingleOrDefaultAsync();

            return agencyInfo.Equals(default)
                ? Result.Failure<AgencyInfo>("Could not find specified agency")
                : agencyInfo;
        }


        public Task<List<AgencyInfo>> GetRootAgencies(string languageCode = LocalizationHelper.DefaultLanguageCode)
            => (
                    from a in _context.Agencies
                    join c in _context.Countries on a.CountryCode equals c.Code
                    where a.ParentId == null
                    select a.ToAgencyInfo(a.ContractKind, a.VerificationState, a.Verified, c.Names, languageCode))
                .ToListAsync();


        public Task<List<AgencyInfo>> GetChildAgencies(int parentAgencyId, string languageCode = LocalizationHelper.DefaultLanguageCode)
            => (
                    from a in _context.Agencies
                    join c in _context.Countries on a.CountryCode equals c.Code
                    join cp in _context.Counterparties on a.CounterpartyId equals cp.Id
                    join ra in _context.Agencies on a.Ancestors[0] equals ra.Id
                    where a.ParentId == parentAgencyId
                    select a.ToAgencyInfo(a.ContractKind, ra.VerificationState, ra.Verified, c.Names, languageCode))
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


        public async Task<Result<AgencyInfo>> Create(RegistrationAgencyInfo agencyInfo, int counterpartyId, int? parentAgencyId)
            => await Create(agencyInfo.Name, counterpartyId, agencyInfo.Address, agencyInfo.BillingEmail, agencyInfo.Fax, 
                agencyInfo.Phone, agencyInfo.PostalCode, agencyInfo.Website, agencyInfo.VatNumber,
                parentAgencyId, agencyInfo.LegalAddress, agencyInfo.PreferredPaymentMethod, agencyInfo.LocalityHtId);


        private async Task<Result<AgencyInfo>> Create(string name, int counterpartyId, string address, string billingEmail, string fax, string phone,
            string postalCode, string website, string vatNumber, int? parentAgencyId, string legalAddress,
            PaymentTypes preferredPaymentMethod, string localityHtId)
        {
            var ancestors = new List<int>();
            var (_, isFailure, localityInfo, error) = await _localityInfoService.GetLocalityInfo(localityHtId);
            if (isFailure)
                return Result.Failure<AgencyInfo>(error);

            if (parentAgencyId is not null)
            {
                var parentAncestors = await _context.Agencies
                    .Where(a => a.Id == parentAgencyId.Value)
                    .Select(a => a.Ancestors ?? new List<int>(0))
                    .SingleAsync();
                
                ancestors.AddRange(parentAncestors);
                ancestors.Add(parentAgencyId.Value);
            }
            
            var now = _dateTimeProvider.UtcNow();
            var agency = new Agency
            {
                Name = name,
                CounterpartyId = counterpartyId,
                Created = now,
                Modified = now,
                ParentId = parentAgencyId,
                Address = address,
                BillingEmail = billingEmail,
                Fax = fax,
                Phone = phone,
                PostalCode = postalCode,
                Website = website,
                VatNumber = vatNumber,
                // Hardcode because we only support USD
                PreferredCurrency = Currencies.USD,
                Ancestors = ancestors,
                LegalAddress = legalAddress,
                PreferredPaymentMethod = preferredPaymentMethod,
                LocalityHtId = localityHtId,
                City = localityInfo.LocalityName,
                CountryCode = localityInfo.CountryIsoCode,
                CountryHtId = localityInfo.CountryHtId
            };
            _context.Agencies.Add(agency);

            await _context.SaveChangesAsync();
            return (await Get(agency.Id)).Value;
        }


        public Task<Result<ContractKind>> GetContractKind(int agencyId)
            => GetRootAgency(agencyId)
                .Ensure(a => a.ContractKind.HasValue, "Agency contract kind unknown")
                .Map(a => a.ContractKind.Value);


        public async Task<Result<AgencyInfo>> Edit(int agencyId, ManagementEditAgencyRequest request, LocalityInfo localityInfo,
            string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            return await GetAgency(agencyId)
                .Tap(Edit)
                .Tap(AddLocalityInfo)
                .Tap(SaveChanges)
                .Bind(GetUpdatedAgencyInfo);


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
        private readonly ILocalityInfoService _localityInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;
    }
}
