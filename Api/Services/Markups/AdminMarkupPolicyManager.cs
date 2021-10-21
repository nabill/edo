using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class AdminMarkupPolicyManager : IAdminMarkupPolicyManager
    {
        public AdminMarkupPolicyManager(EdoContext context,
            IMarkupPolicyTemplateService templateService,
            IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService,
            IAdministratorContext administratorContext,
            IMarkupPolicyAuditService markupPolicyAuditService,
            IAccommodationMapperClient mapperClient)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _administratorContext = administratorContext;
            _markupPolicyAuditService = markupPolicyAuditService;
            _mapperClient = mapperClient;
        }
        
        
        public async Task<Result> Add(MarkupPolicyData policyData)
        {
            var destinationScopeType = await GetDestinationScopeType(policyData.Settings.DestinationScopeId);
            if (destinationScopeType.IsFailure)
                return Result.Failure(destinationScopeType.Error);
            
            var (_, isFailure, markupPolicy, error) = await ValidatePolicy(policyData)
                .Map(SavePolicy)
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Created));

            if (isFailure)
                return Result.Failure(error);
            
            return await UpdateDisplayedMarkupFormula(markupPolicy);


            async Task<MarkupPolicy> SavePolicy()
            {
                var now = _dateTimeProvider.UtcNow();
                var (type, counterpartyId, agencyId, agentId, agentScopeId) = policyData.Scope;
                var settings = policyData.Settings;
                
                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    Order = settings.Order,
                    Target = policyData.Target,
                    TemplateSettings = settings.TemplateSettings,
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    TemplateId = settings.TemplateId,
                    AgentScopeType = type,
                    AgentScopeId = agentScopeId,
                    DestinationScopeId = settings.DestinationScopeId,
                    DestinationScopeType = destinationScopeType.Value
                };

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }


        public async Task<Result> Remove(int policyId)
        {
            var (_, isFailure, markupPolicy, error) = await GetPolicy()
                .Ensure(HasNoDiscounts, "Markup policy has bound discounts")
                .Map(DeletePolicy)
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Deleted));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);


            async Task<Result<MarkupPolicy>> GetPolicy()
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
                return policy == null
                    ? Result.Failure<MarkupPolicy>("Could not find policy")
                    : Result.Success(policy);
            }


            async Task<bool> HasNoDiscounts(MarkupPolicy policy)
                => !await _context.Discounts.AnyAsync(d => d.TargetPolicyId == policy.Id);


            async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }

        public async Task<Result> Modify(int policyId, MarkupPolicySettings settings)
        {
            var destinationScopeType = await GetDestinationScopeType(settings.DestinationScopeId);
            if (destinationScopeType.IsFailure)
                return Result.Failure(destinationScopeType.Error);
            
            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure("Could not find policy");

            var (_, isFailure, markupPolicy, error) = await ValidateSettings()
                .Bind(DiscountsDontExceedMarkups)
                .Bind(UpdatePolicy)
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Modified));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);


            Result ValidateSettings() => _templateService.Validate(settings.TemplateId, settings.TemplateSettings);
            
            
            async Task<Result> DiscountsDontExceedMarkups()
            {
                // This check is only applicable to agency scope markups
                if (policy.AgentScopeType != AgentMarkupScopeTypes.Agency)
                    return Result.Success();

                var agencyId = int.Parse(policy.AgentScopeId);
                
                var allDiscounts = await _context.Discounts
                    .Where(x => x.IsActive)
                    .Where(x => x.TargetAgencyId == agencyId)
                    .Where(x => x.TargetPolicyId == policy.Id)
                    .Select(x => x.DiscountPercent)
                    .ToListAsync();

                var markupFunction = _templateService.CreateFunction(policy.TemplateId, policy.TemplateSettings);
                return DiscountsValidator.DiscountsDontExceedMarkups(allDiscounts, markupFunction);
            }


            async Task<Result<MarkupPolicy>> UpdatePolicy()
            {
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();
                policy.DestinationScopeId = settings.DestinationScopeId;
                policy.DestinationScopeType = destinationScopeType.Value;

                var (_, isFailure, error) = await ValidatePolicy(GetPolicyData(policy), policy);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }


        public async Task<List<MarkupPolicyData>> Get(MarkupPolicyScope scope)
        {
            return (await GetPoliciesForScope(scope))
                .Select(GetPolicyData)
                .ToList();
        }


        public Task<List<MarkupInfo>> GetGlobalPolicies()
        {
            return _context.MarkupPolicies
                .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Global)
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }


        public Task<Result> AddGlobalPolicy(MarkupPolicySettings settings)
            => Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings, new MarkupPolicyScope(AgentMarkupScopeTypes.Global)));


        public async Task<Result> RemoveGlobalPolicy(int policyId)
        {
            var isGlobalPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Global &&
                    p.Id == policyId);
            
            return isGlobalPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' not found or not global");
        }


        public async Task<Result> ModifyGlobalPolicy(int policyId, MarkupPolicySettings settings)
        {
            var isGlobalPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Global &&
                    p.Id == policyId);
            
            return isGlobalPolicy
                ? await Modify(policyId, settings)
                : Result.Failure($"Policy '{policyId}' not found or not global");
        }


        public Task<Result> AddCounterpartyPolicy(int counterpartyId, MarkupPolicySettings settings) 
            => Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings, new MarkupPolicyScope(AgentMarkupScopeTypes.Counterparty, counterpartyId)));


        public async Task<Result> RemoveFromCounterparty(int policyId, int counterpartyId)
        {
            var isCounterpartyPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Counterparty &&
                    p.AgentScopeId == counterpartyId.ToString() &&
                    p.Id == policyId);

            return isCounterpartyPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' isn't applied to the counterparty '{counterpartyId}'");
        }


        public async Task<Result> ModifyCounterpartyPolicy(int policyId, int counterpartyId, MarkupPolicySettings settings)
        {
            var agentScopeId = counterpartyId.ToString();
            var isCounterpartyPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Counterparty &&
                    p.AgentScopeId == agentScopeId &&
                    p.Id == policyId);

            return isCounterpartyPolicy
                ? await Modify(policyId, settings) 
                : Result.Failure($"Policy '{policyId}' isn't applied to the counterparty '{counterpartyId}'");
        }


        public Task<List<MarkupInfo>> GetMarkupsForCounterparty(int counterpartyId)
        {
            return _context.MarkupPolicies
                .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Counterparty && p.AgentScopeId == counterpartyId.ToString())
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }


        public Task<List<MarkupInfo>> GetLocationPolicies()
        {
            return _context.MarkupPolicies
                .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Country || p.AgentScopeType == AgentMarkupScopeTypes.Locality)
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }

        
        public async Task<Result> AddLocationPolicy(MarkupPolicySettings settings)
        {
            var (_, isFailure, agentMarkupScopeType, error) = await GetAgentMarkupScopeType(settings.LocationScopeId);
            if (isFailure)
                return Result.Failure(error);
                
            return await Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings,
                new MarkupPolicyScope(agentMarkupScopeType, locationId: settings.LocationScopeId)));
        }


        public async Task<Result> ModifyLocationPolicy(int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId);

            if (policy.AgentScopeType is not (AgentMarkupScopeTypes.Country or AgentMarkupScopeTypes.Locality))
                return Result.Failure($"Policy '{policyId}' not found or not local");
            
            var (_, isFailure, agentMarkupScopeType, error) = await GetAgentMarkupScopeType(settings.LocationScopeId);
            if (isFailure)
                return Result.Failure(error);
            
            if (policy.AgentScopeType != agentMarkupScopeType)
                return Result.Failure($"It is not allowed change location to a new type");
            
            return await Modify(policyId, settings);
        }


        public async Task<Result> RemoveLocationPolicy(int policyId)
        {
            var isLocationPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    (p.AgentScopeType == AgentMarkupScopeTypes.Country || p.AgentScopeType == AgentMarkupScopeTypes.Locality) &&
                    p.Id == policyId);
            
            return isLocationPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' not found or not local");
        }


        private Task<List<MarkupPolicy>> GetPoliciesForScope(MarkupPolicyScope scope)
        {
            var (agentScopeType, counterpartyId, agencyId, agentId, agentScopeId) = scope;
            return agentScopeType switch
            {
                AgentMarkupScopeTypes.Global => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Global)
                    .ToListAsync(),
                AgentMarkupScopeTypes.Country => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Country)
                    .ToListAsync(),
                AgentMarkupScopeTypes.Locality => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Locality)
                    .ToListAsync(),
                AgentMarkupScopeTypes.Counterparty => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Counterparty && p.AgentScopeId == agentScopeId)
                    .ToListAsync(),
                AgentMarkupScopeTypes.Agency => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId == agentScopeId)
                    .ToListAsync(),
                AgentMarkupScopeTypes.Agent => _context.MarkupPolicies
                    .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Agent && p.AgentScopeId == agentScopeId)
                    .ToListAsync(),
                _ => Task.FromResult(new List<MarkupPolicy>(0))
            };
        }


        private static MarkupPolicyData GetPolicyData(MarkupPolicy policy)
        {
            int? counterpartyId = null, agencyId = null, agentId = null;
            
            if (policy.AgentScopeType == AgentMarkupScopeTypes.Counterparty)
                counterpartyId = int.Parse(policy.AgentScopeId);
            
            if (policy.AgentScopeType == AgentMarkupScopeTypes.Agency)
                agencyId = int.Parse(policy.AgentScopeId);

            if (policy.AgentScopeType == AgentMarkupScopeTypes.Agency)
            {
                var agentInAgencyId = AgentInAgencyId.Create(policy.AgentScopeId);
                agencyId = agentInAgencyId.AgencyId;
                agentId = agentInAgencyId.AgentId;
            }

            return new MarkupPolicyData(policy.Target,
                new MarkupPolicySettings(policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order, policy.Currency, policy.DestinationScopeId),
                new MarkupPolicyScope(policy.AgentScopeType, counterpartyId, agencyId, agentId));
        }


        private Task<Result> ValidatePolicy(MarkupPolicyData policyData, MarkupPolicy sourcePolicy = null)
        {
            return ValidateTemplate()
                .Ensure(ScopeIsValid, "Invalid scope data")
                .Ensure(TargetIsValid, "Invalid policy target")
                .Ensure(PolicyOrderIsUniqueForScope, "Policy with same order is already defined");


            Result ValidateTemplate() => _templateService.Validate(policyData.Settings.TemplateId, policyData.Settings.TemplateSettings);


            bool ScopeIsValid() => policyData.Scope.Validate().IsSuccess;


            bool TargetIsValid() => policyData.Target != MarkupPolicyTarget.NotSpecified;

            
            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                if (sourcePolicy is not null && sourcePolicy.Order == policyData.Settings.Order)
                    return true;
                
                var isSameOrderPolicyExist = (await GetPoliciesForScope(policyData.Scope))
                    .Any(p => p.Order == policyData.Settings.Order);

                return !isSameOrderPolicyExist;
            }
        }
        
        
        private async Task<Result<AgentMarkupScopeTypes>> GetAgentMarkupScopeType(string locationId)
        {
            var (_, isFailure, value, error) = await _mapperClient.GetMappings(new List<string> { locationId }, "en");
            
            if (isFailure)
                return Result.Failure<AgentMarkupScopeTypes>(error.Detail);
        
            if (!value.Any())
                return Result.Failure<AgentMarkupScopeTypes>("Provided location scope id does not exist");
        
            return value.Single().Location.Type switch
            {
                MapperLocationTypes.Country => AgentMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => AgentMarkupScopeTypes.Locality,
                _ => Result.Failure<AgentMarkupScopeTypes>("Not implemented location type for provided location scope id")
            };
        }
        
        
        private async Task<Result<DestinationMarkupScopeTypes>> GetDestinationScopeType(string destinationScopeId)
        {
            if (string.IsNullOrWhiteSpace(destinationScopeId))
                return DestinationMarkupScopeTypes.Global;
            
            var (_, isFailure, value, error) = await _mapperClient.GetMappings(new List<string> { destinationScopeId }, "en");
            
            if (isFailure)
                return Result.Failure<DestinationMarkupScopeTypes>(error.Detail);

            if (!value.Any())
                return Result.Failure<DestinationMarkupScopeTypes>("Provided destination scope id does not exist");

            return value.Single().Location.Type switch
            {
                MapperLocationTypes.Country => DestinationMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => DestinationMarkupScopeTypes.Locality,
                MapperLocationTypes.Accommodation => DestinationMarkupScopeTypes.Accommodation,
                _ => Result.Failure<DestinationMarkupScopeTypes>("Not implemented destination type for provided destinatio scope id")
            };
        }
        
        
        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
        {
            return policy.AgentScopeType switch
            {
                AgentMarkupScopeTypes.Agent
                    => _displayedMarkupFormulaService.UpdateAgentFormula(AgentInAgencyId.Create(policy.AgentScopeId).AgentId, AgentInAgencyId.Create(policy.AgentScopeId).AgencyId),
                
                AgentMarkupScopeTypes.Agency
                    => _displayedMarkupFormulaService.UpdateAgencyFormula(int.Parse(policy.AgentScopeId)),
                
                AgentMarkupScopeTypes.Counterparty
                    => _displayedMarkupFormulaService.UpdateCounterpartyFormula(int.Parse(policy.AgentScopeId)),
                
                AgentMarkupScopeTypes.Global
                    => _displayedMarkupFormulaService.UpdateGlobalFormula(),

                _ => Task.FromResult(Result.Success())
            };
        }


        private async Task WriteAuditLog(MarkupPolicy policy, MarkupPolicyEventOperationType type)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            
            var writeLogTask = (policy.AgentScopeType, type) switch
            {
                (AgentMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Created) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupCreated),
                (AgentMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Modified) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupUpdated),
                (AgentMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Deleted) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupDeleted),
                (AgentMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Created) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupCreated),
                (AgentMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Modified) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupUpdated),
                (AgentMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Deleted) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupDeleted),
                (AgentMarkupScopeTypes.Counterparty, MarkupPolicyEventOperationType.Created) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupCreated),
                (AgentMarkupScopeTypes.Counterparty, MarkupPolicyEventOperationType.Modified) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupUpdated),
                (AgentMarkupScopeTypes.Counterparty, MarkupPolicyEventOperationType.Deleted) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupDeleted),
                (AgentMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Created) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupCreated),
                (AgentMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Modified) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupUpdated),
                (AgentMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Deleted) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupDeleted),
                _ => Task.CompletedTask
            };

            await writeLogTask;
            
            
            Task WriteAgentLog(MarkupPolicyEventType eventType)
            {
                var agentInAgencyId = AgentInAgencyId.Create(policy.AgentScopeId);
                return _markupPolicyAuditService.Write(eventType, new AgentMarkupPolicyData(policy.Id, agentInAgencyId.AgentId, agentInAgencyId.AgencyId), administrator.ToApiCaller());
            }


            Task WriteAgencyLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.AgentScopeId)), administrator.ToApiCaller());


            Task WriteCounterpartyLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new CounterpartyMarkupPolicyData(policy.Id, int.Parse(policy.AgentScopeId)), administrator.ToApiCaller());


            Task WriteGlobalLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new GlobalMarkupPolicyData(policy.Id), administrator.ToApiCaller());
        }


        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IAdministratorContext _administratorContext;
        private readonly IMarkupPolicyAuditService _markupPolicyAuditService;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}