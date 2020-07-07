using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Data.Management;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.AdministratorServices
{
    public class CounterpartyManagementService : ICounterpartyManagementService
    {
        public CounterpartyManagementService(EdoContext context, IDateTimeProvider dateTimeProvider, IManagementAuditService managementAuditService,
            IAgentPermissionManagementService<Administrator> permissionManagementService, IAccountManagementService accountManagementService)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
            _permissionManagementService = permissionManagementService;
            _accountManagementService = accountManagementService;
        }


        public async Task<Result<CounterpartyInfo>> Get(int counterpartyId, string languageCode)
        {
            var counterpartyData = await (from cp in _context.Counterparties
                join c in _context.Countries
                    on cp.CountryCode equals c.Code
                where cp.Id == counterpartyId
                select new
                {
                    Counterparty = cp,
                    Country = c
                }).SingleOrDefaultAsync();
            if (counterpartyData == default)
                return Result.Failure<CounterpartyInfo>("Could not find counterparty with specified id");

            return ToCounterpartyInfo(counterpartyData.Counterparty, counterpartyData.Country, languageCode);
        }


        public async Task<List<CounterpartyInfo>> Get(string languageCode)
        {
            var counterparties = await (from cp in _context.Counterparties
                join c in _context.Countries
                    on cp.CountryCode equals c.Code
                select new
                {
                    Counterparty = cp,
                    Country = c
                }).ToListAsync();

            return counterparties.Select(c => ToCounterpartyInfo(c.Counterparty, c.Country, languageCode)).ToList();
        }


        public Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId)
        {
            return GetCounterparty(counterpartyId)
                .Map(counterparty => GetAgencies());


            Task<List<AgencyInfo>> GetAgencies()
                => _context.Agencies.Where(a => a.CounterpartyId == counterpartyId)
                    .Select(b => new AgencyInfo(b.Name, b.Id)).ToListAsync();
        }


        public Task<Result<CounterpartyInfo>> Update(CounterpartyEditRequest changedCounterpartyInfo, int counterpartyId, string languageCode)
        {
            return GetCounterparty(counterpartyId)
                .Bind(UpdateCounterparty);


            async Task<Result<CounterpartyInfo>> UpdateCounterparty(Counterparty counterpartyToUpdate)
            {
                var (_, isFailure, error) = CounterpartyValidator.Validate(changedCounterpartyInfo);
                if (isFailure)
                    return Result.Failure<CounterpartyInfo>(error);

                counterpartyToUpdate.Address = changedCounterpartyInfo.Address;
                counterpartyToUpdate.City = changedCounterpartyInfo.City;
                counterpartyToUpdate.CountryCode = changedCounterpartyInfo.CountryCode;
                counterpartyToUpdate.Fax = changedCounterpartyInfo.Fax;
                counterpartyToUpdate.Name = changedCounterpartyInfo.Name;
                counterpartyToUpdate.Phone = changedCounterpartyInfo.Phone;
                counterpartyToUpdate.Website = changedCounterpartyInfo.Website;
                counterpartyToUpdate.PostalCode = changedCounterpartyInfo.PostalCode;
                counterpartyToUpdate.PreferredCurrency = changedCounterpartyInfo.PreferredCurrency;
                counterpartyToUpdate.PreferredPaymentMethod = changedCounterpartyInfo.PreferredPaymentMethod;
                counterpartyToUpdate.Updated = _dateTimeProvider.UtcNow();
                counterpartyToUpdate.VatNumber = changedCounterpartyInfo.VatNumber;
                counterpartyToUpdate.BillingEmail = changedCounterpartyInfo.BillingEmail;

                _context.Counterparties.Update(counterpartyToUpdate);
                await _context.SaveChangesAsync();

                return await Get(counterpartyId, languageCode);
            }
        }


        public Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason)
        {
            return Verify(counterpartyId, counterparty => Result.Ok(counterparty)
                .Tap(c => SetVerified(c, CounterpartyStates.FullAccess, verificationReason))
                .Bind(_ => Task.FromResult(Result.Ok())) // HACK: conversion hack because can't map tasks
                .Bind(() => SetPermissions(counterpartyId, GetPermissionSet))
                .Tap(() => WriteToAuditLog(counterpartyId, verificationReason)));


            static InAgencyPermissions GetPermissionSet(bool isMaster)
                => isMaster
                    ? PermissionSets.FullAccessMaster
                    : PermissionSets.FullAccessDefault;
        }


        public Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason)
        {
            return Verify(counterpartyId, counterparty => Result.Ok(counterparty)
                .Tap(c => SetVerified(c, CounterpartyStates.ReadOnly, verificationReason))
                .BindWithTransaction(_context, c =>
                    CreatePaymentAccountForCounterparty(c)
                        .Bind(() => CreatePaymentAccountsForAgencies(c)))
                .Bind(() => SetPermissions(counterpartyId, GetPermissionSet))
                .Tap(() => WriteToAuditLog(counterpartyId, verificationReason)));


            Task<Result> CreatePaymentAccountForCounterparty(Counterparty counterparty)
                => _accountManagementService
                    .CreateForCounterparty(counterparty, counterparty.PreferredCurrency);


            async Task<Result> CreatePaymentAccountsForAgencies(Counterparty counterparty)
            {
                var agencies = await _context.Agencies.Where(a => a.CounterpartyId == counterpartyId).ToListAsync();

                foreach (var agency in agencies)
                {
                    var (_, isFailure) = await _accountManagementService.CreateForAgency(agency, counterparty.PreferredCurrency);
                    if (isFailure)
                        return Result.Failure("Error while creating accounts for agencies");
                }

                return Result.Ok();
            }


            static InAgencyPermissions GetPermissionSet(bool isMaster)
                => isMaster
                    ? PermissionSets.ReadOnlyMaster
                    : PermissionSets.ReadOnlyDefault;
        }


        // This method is the same with CounterpartyService.GetCounterparty,
        // because administrator services in the future will be replaced to another application
        private async Task<Result<Counterparty>> GetCounterparty(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);
            if (counterparty == null)
                return Result.Failure<Counterparty>("Could not find counterparty with specified id");

            return Result.Ok(counterparty);
        }


        private Task<List<AgentContainer>> GetAgents(int counterpartyId)
            => (from rel in _context.AgentAgencyRelations
                    join ag in _context.Agencies on rel.AgencyId equals ag.Id
                    where ag.CounterpartyId == counterpartyId
                    select new AgentContainer(rel.AgentId, rel.AgencyId, rel.Type))
                .ToListAsync();


        private async Task<Result> SetPermissions(int counterpartyId, Func<bool, InAgencyPermissions> isMasterCondition)
        {
            foreach (var agent in await GetAgents(counterpartyId))
            {
                var permissions = isMasterCondition.Invoke(agent.Type == AgentAgencyRelationTypes.Master);
                var (_, isFailure, _, error) = await _permissionManagementService.SetInAgencyPermissions(agent.AgencyId, agent.Id, permissions);
                if (isFailure)
                    return Result.Failure(error);
            }

            return Result.Ok();
        }


        private Task SetVerified(Counterparty counterparty, CounterpartyStates state, string verificationReason)
        {
            var now = _dateTimeProvider.UtcNow();
            string reason;
            if (string.IsNullOrEmpty(counterparty.VerificationReason))
                reason = verificationReason;
            else
                reason = counterparty.VerificationReason + Environment.NewLine + verificationReason;

            counterparty.State = state;
            counterparty.VerificationReason = reason;
            counterparty.Verified = now;
            counterparty.Updated = now;
            _context.Update(counterparty);

            return _context.SaveChangesAsync();
        }


        private Task<Result> Verify(int counterpartyId, Func<Counterparty, Task<Result>> verificationFunc)
        {
            return GetCounterparty(counterpartyId)
                .BindWithTransaction(_context, verificationFunc);
        }


        private Task WriteToAuditLog(int counterpartyId, string verificationReason)
            => _managementAuditService.Write(ManagementEventType.CounterpartyVerification,
                new CounterpartyVerifiedAuditEventData(counterpartyId, verificationReason));


        private static CounterpartyInfo ToCounterpartyInfo(Counterparty counterparty, Country country, string languageCode)
            => new CounterpartyInfo(
                counterparty.Id,
                counterparty.Name,
                counterparty.Address,
                counterparty.CountryCode,
                LocalizationHelper.GetValueFromSerializedString(country.Names, languageCode),
                counterparty.City,
                counterparty.Phone,
                counterparty.Fax,
                counterparty.PostalCode,
                counterparty.PreferredCurrency,
                counterparty.PreferredPaymentMethod,
                counterparty.Website,
                counterparty.VatNumber,
                counterparty.BillingEmail);


        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IManagementAuditService _managementAuditService;
        private readonly IAgentPermissionManagementService<Administrator> _permissionManagementService;


        private readonly struct AgentContainer
        {
            public AgentContainer(int id, int agencyId, AgentAgencyRelationTypes type)
            {
                Id = id;
                AgencyId = agencyId;
                Type = type;
            }


            public int Id { get; }
            public int AgencyId { get; }
            public AgentAgencyRelationTypes Type { get; }
        }
    }
}