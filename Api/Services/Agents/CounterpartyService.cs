using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class CounterpartyService : ICounterpartyService
    {
        public CounterpartyService(EdoContext context,
            IAccountManagementService accountManagementService,
            IDateTimeProvider dateTimeProvider,
            IManagementAuditService managementAuditService,
            IAgentContext agentContext, 
            IAgentPermissionManagementService permissionManagementService)
        {
            _context = context;
            _accountManagementService = accountManagementService;
            _dateTimeProvider = dateTimeProvider;
            _managementAuditService = managementAuditService;
            _agentContext = agentContext;
            _permissionManagementService = permissionManagementService;
        }


        public async Task<Result<Counterparty>> Add(CounterpartyInfo counterparty)
        {
            var (_, isFailure, error) = Validate(counterparty);
            if (isFailure)
                return Result.Fail<Counterparty>(error);

            var now = _dateTimeProvider.UtcNow();
            var createdCounterparty = new Counterparty
            {
                Address = counterparty.Address,
                City = counterparty.City,
                CountryCode = counterparty.CountryCode,
                Fax = counterparty.Fax,
                Name = counterparty.Name,
                Phone = counterparty.Phone,
                Website = counterparty.Website,
                PostalCode = counterparty.PostalCode,
                PreferredCurrency = counterparty.PreferredCurrency,
                PreferredPaymentMethod = counterparty.PreferredPaymentMethod,
                State = CounterpartyStates.PendingVerification,
                Created = now,
                Updated = now
            };

            _context.Counterparties.Add(createdCounterparty);
            await _context.SaveChangesAsync();
            
            var defaultAgency = new Agency
            {
                Name = createdCounterparty.Name,
                CounterpartyId = createdCounterparty.Id,
                IsDefault = true,
                Created = now,
                Modified = now,
            };
            _context.Agencies.Add(defaultAgency);
            
            await _context.SaveChangesAsync();
            return Result.Ok(createdCounterparty);
        }


        public Task<Result<CounterpartyInfo>> Get(int counterpartyId)
        {
            return GetCounterpartyForAgent(counterpartyId)
                .OnSuccess(counterparty => new CounterpartyInfo(
                    counterparty.Name,
                    counterparty.Address,
                    counterparty.CountryCode,
                    counterparty.City,
                    counterparty.Phone,
                    counterparty.Fax,
                    counterparty.PostalCode,
                    counterparty.PreferredCurrency,
                    counterparty.PreferredPaymentMethod,
                    counterparty.Website));
        }


        public Task<Result<CounterpartyInfo>> Update(CounterpartyInfo changedCounterpartyInfo, int counterpartyId)
        {
            return GetCounterpartyForAgent(counterpartyId)
                .OnSuccess(UpdateCounterparty);

            async Task<Result<CounterpartyInfo>> UpdateCounterparty(Counterparty counterpartyToUpdate)
            {
                var (_, isFailure, error) = Validate(changedCounterpartyInfo);
                if (isFailure)
                    return Result.Fail<CounterpartyInfo>(error);

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

                _context.Counterparties.Update(counterpartyToUpdate);
                await _context.SaveChangesAsync();

                return Result.Ok(new CounterpartyInfo(
                    counterpartyToUpdate.Name,
                    counterpartyToUpdate.Address,
                    counterpartyToUpdate.CountryCode,
                    counterpartyToUpdate.City,
                    counterpartyToUpdate.Phone,
                    counterpartyToUpdate.Fax,
                    counterpartyToUpdate.PostalCode,
                    counterpartyToUpdate.PreferredCurrency,
                    counterpartyToUpdate.PreferredPaymentMethod,
                    counterpartyToUpdate.Website));
            }
        }


        public Task<Result<Agency>> AddAgency(int counterpartyId, AgencyInfo agency)
        {
            return CheckCounterpartyExists()
                .Ensure(HasPermissions, "Permission to create agencies denied")
                .Ensure(IsAgencyNameUnique, $"Agency with name {agency.Name} already exists")
                .OnSuccess(SaveAgency);


            async Task<bool> HasPermissions()
            {
                var agentInfo = await _agentContext.GetAgent();
                return agentInfo.IsMaster && agentInfo.CounterpartyId == counterpartyId;
            }


            async Task<Result> CheckCounterpartyExists()
            {
                return await _context.Counterparties.AnyAsync(c => c.Id == counterpartyId)
                    ? Result.Ok()
                    : Result.Fail("Could not find counterparty with specified id");
            }


            async Task<bool> IsAgencyNameUnique()
            {
                return !await _context.Agencies.Where(a => a.CounterpartyId == counterpartyId &&
                        EF.Functions.ILike(a.Name, agency.Name))
                    .AnyAsync();
            }

            
            async Task<Agency> SaveAgency()
            {
                var now = _dateTimeProvider.UtcNow();
                var createdAgency = new Agency
                {
                    Name = agency.Name,
                    CounterpartyId = counterpartyId,
                    IsDefault = false,
                    Created = now,
                    Modified = now,
                };
                _context.Agencies.Add(createdAgency);
                await _context.SaveChangesAsync();

                return createdAgency;
            }
        }


        public Task<Result<AgencyInfo>> GetAgency(int counterpartyId, int agencyId)
        {
            return GetCounterpartyForAgent(counterpartyId)
                .OnSuccess(GetAgency);

            async Task<Result<AgencyInfo>> GetAgency()
            {
                var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
                if (agency == null)
                    return Result.Fail<AgencyInfo>("Could not find agency with specified id");
                
                return Result.Ok(new AgencyInfo(agency.Name, agency.Id));
            }
        }


        public Task<Result<List<AgencyInfo>>> GetAllCounterpartyAgencies(int counterpartyId)
        {
            return GetCounterpartyForAgent(counterpartyId)
                .OnSuccess(GetAgencies);

            async Task<Result<List<AgencyInfo>>> GetAgencies() => 
                Result.Ok(
                    await _context.Agencies.Where(a => a.CounterpartyId == counterpartyId)
                    .Select(b => new AgencyInfo(b.Name, b.Id)).ToListAsync());
        }


        public Task<Agency> GetDefaultAgency(int counterpartyId)
            => _context.Agencies
                .SingleAsync(a => a.CounterpartyId == counterpartyId && a.IsDefault);


        public Task<Result> VerifyAsFullyAccessed(int counterpartyId, string verificationReason)
        {
            return Verify(counterpartyId, counterparty => Result.Ok(counterparty)
                    .OnSuccess(c => SetVerified(c, CounterpartyStates.FullAccess, verificationReason))
                    .OnSuccess(_ => Task.FromResult(Result.Ok())) // HACK: conversion hack because can't map tasks
                    .OnSuccess(() => SetPermissions(counterpartyId, GetPermissionSet))
                    .OnSuccess(() => WriteToAuditLog(counterpartyId, verificationReason)));


            InCounterpartyPermissions GetPermissionSet(bool isMaster)
                => isMaster 
                    ? PermissionSets.FullAccessMaster 
                    : PermissionSets.FullAccessDefault;
        }


        public Task<Result> VerifyAsReadOnly(int counterpartyId, string verificationReason)
        {
            return Verify(counterpartyId, counterparty => Result.Ok(counterparty)
                    .OnSuccess(c => SetVerified(c, CounterpartyStates.ReadOnly, verificationReason))
                    .OnSuccess(CreatePaymentAccount)
                    .OnSuccess(() => SetPermissions(counterpartyId, GetPermissionSet))
                    .OnSuccess(() => WriteToAuditLog(counterpartyId, verificationReason)));


            Task<Result> CreatePaymentAccount(Counterparty counterparty)
                => _accountManagementService
                    .Create(counterparty, counterparty.PreferredCurrency);


            InCounterpartyPermissions GetPermissionSet(bool isMaster)
                => isMaster 
                    ? PermissionSets.ReadOnlyMaster 
                    : PermissionSets.ReadOnlyDefault;
        }


        private Task<List<AgentContainer>> GetAgents(int counterpartyId)
            => _context.AgentCounterpartyRelations
                .Where(r => r.CounterpartyId == counterpartyId)
                .Select(r => new AgentContainer(r.AgentId, r.AgencyId, r.Type))
                .ToListAsync();


        private async Task<Result> SetPermissions(int counterpartyId, Func<bool, InCounterpartyPermissions> isMasterCondition)
        {
            foreach (var agent in await GetAgents(counterpartyId))
            {
                var permissions = isMasterCondition.Invoke(agent.Type == AgentCounterpartyRelationTypes.Master);
                var (_, isFailure, _, error) = await _permissionManagementService.SetInCounterpartyPermissions(counterpartyId, agent.AgencyId, agent.Id, permissions);
                if (isFailure)
                    return Result.Fail(error);
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
            return GetCounterparty()
                .OnSuccessWithTransaction(_context, verificationFunc);

            async Task<Result<Counterparty>> GetCounterparty()
            {
                var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);
                return counterparty == default
                    ? Result.Fail<Counterparty>($"Could not find counterparty with id {counterpartyId}")
                    : Result.Ok(counterparty);
            }
        }


        private async Task<Result<Counterparty>> GetCounterpartyForAgent(int counterpartyId)
        {
            var (_, agentCounterpartyId, _, _) = await _agentContext.GetAgent();

            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(c => c.Id == counterpartyId);
            if (counterparty == null)
                return Result.Fail<Counterparty>("Could not find counterparty with specified id");

            if (agentCounterpartyId != counterpartyId)
                return Result.Fail<Counterparty>("The agent isn't affiliated with the counterparty");

            return Result.Ok(counterparty);
        }


        private static Result Validate(in CounterpartyInfo counterpartyInfo)
        {
            return GenericValidator<CounterpartyInfo>.Validate(v =>
            {
                v.RuleFor(c => c.Name).NotEmpty();
                v.RuleFor(c => c.Address).NotEmpty();
                v.RuleFor(c => c.City).NotEmpty();
                v.RuleFor(c => c.Phone).NotEmpty().Matches(@"^[0-9]{3,30}$");
                v.RuleFor(c => c.Fax).Matches(@"^[0-9]{3,30}$").When(i => !string.IsNullOrWhiteSpace(i.Fax));
            }, counterpartyInfo);
        }


        private Task WriteToAuditLog(int counterpartyId, string verificationReason) 
            => _managementAuditService.Write(ManagementEventType.CounterpartyVerification, new CounterpartyVerifiedAuditEventData(counterpartyId, verificationReason));


        private readonly IAccountManagementService _accountManagementService;
        private readonly EdoContext _context;
        private readonly IAgentContext _agentContext;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAgentPermissionManagementService _permissionManagementService;
        private readonly IManagementAuditService _managementAuditService;


        private readonly struct AgentContainer
        {
            public AgentContainer(int id, int agencyId, AgentCounterpartyRelationTypes type)
            {
                Id = id;
                AgencyId = agencyId;
                Type = type;
            }


            public int Id { get; }
            public int AgencyId { get; }
            public AgentCounterpartyRelationTypes Type { get; }
        }
    }
}