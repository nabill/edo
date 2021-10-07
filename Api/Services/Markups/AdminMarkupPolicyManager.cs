using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
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
                
                // TODO remove after completing migration to new markups
                var scopeType = type switch 
                {
                    AgentMarkupScopeTypes.Agency => MarkupPolicyScopeType.Agency,
                    AgentMarkupScopeTypes.Agent => MarkupPolicyScopeType.Agent,
                    AgentMarkupScopeTypes.Counterparty => MarkupPolicyScopeType.Counterparty,
                    AgentMarkupScopeTypes.Global => MarkupPolicyScopeType.Global,
                    _ => MarkupPolicyScopeType.NotSpecified
                };

                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    Order = settings.Order,
                    Target = policyData.Target,
                    TemplateSettings = settings.TemplateSettings,
                    ScopeType = scopeType,
                    CounterpartyId = counterpartyId,
                    AgencyId = agencyId,
                    AgentId = agentId,
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    TemplateId = settings.TemplateId,
                    AgentScopeType = type,
                    AgentScopeId = agentScopeId,
                    DestinationScopeId = settings.DestinationScopeId
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
                var allDiscounts = await _context.Discounts
                    .Where(x => x.IsActive)
                    .Where(x => x.TargetAgencyId == policy.AgencyId)
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
                .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Location)
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }

        
        public Task<Result> AddLocationPolicy(MarkupPolicySettings settings)
        {
            return Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings,
                new MarkupPolicyScope(AgentMarkupScopeTypes.Location, locationId: settings.LocationScopeId)));
        }


        public async Task<Result> ModifyLocationPolicy(int policyId, MarkupPolicySettings settings)
        {
            var isLocationPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Location &&
                    p.Id == policyId);
            
            return isLocationPolicy
                ? await Modify(policyId, settings)
                : Result.Failure($"Policy '{policyId}' not found or not local");
        }


        public async Task<Result> RemoveLocationPolicy(int policyId)
        {
            var isLocationPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.AgentScopeType == AgentMarkupScopeTypes.Location &&
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
            return new MarkupPolicyData(policy.Target,
                new MarkupPolicySettings(policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order, policy.Currency, policy.DestinationScopeId),
                new MarkupPolicyScope(policy.AgentScopeType, policy.CounterpartyId, policy.AgencyId, policy.AgentId));
        }


        private Task<Result> ValidatePolicy(MarkupPolicyData policyData, MarkupPolicy sourcePolicy = null)
        {
            return ValidateTemplate()
                .Ensure(ScopeIsValid, "Invalid scope data")
                .Ensure(TargetIsValid, "Invalid policy target")
                .Ensure(LocationScopeIdExists, "Provided location scope id does not exist")
                .Ensure(DestinationScopeIdExists, "Provided destination scope id does not exist")
                .Ensure(PolicyOrderIsUniqueForScope, "Policy with same order is already defined");


            Result ValidateTemplate() => _templateService.Validate(policyData.Settings.TemplateId, policyData.Settings.TemplateSettings);


            bool ScopeIsValid() => policyData.Scope.Validate().IsSuccess;


            bool TargetIsValid() => policyData.Target != MarkupPolicyTarget.NotSpecified;

            
            // TODO: use method for check existence of destination when it will be ready https://github.com/happy-travel/agent-app-project/issues/667
            async Task<bool> DestinationScopeIdExists()
            {
                var destinationScopeId = policyData.Settings.DestinationScopeId;
                if (string.IsNullOrWhiteSpace(destinationScopeId))
                    return true;
                
                var (isSuccess, _, value) = await _mapperClient.GetMappings(new List<string> { destinationScopeId }, "en");
                return isSuccess && value.Any();
            }
            
            
            // TODO: use method for check existence of destination when it will be ready https://github.com/happy-travel/agent-app-project/issues/667
            async Task<bool> LocationScopeIdExists()
            {
                var locationScopeId = policyData.Settings.LocationScopeId;
                if (string.IsNullOrWhiteSpace(locationScopeId))
                    return true;
                
                var (isSuccess, _, value) = await _mapperClient.GetMappings(new List<string> { locationScopeId }, "en");
                return isSuccess && value.Any();
            }

            
            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                if (sourcePolicy is not null && sourcePolicy.Order == policyData.Settings.Order)
                    return true;
                
                var isSameOrderPolicyExist = (await GetPoliciesForScope(policyData.Scope))
                    .Any(p => p.Order == policyData.Settings.Order);

                return !isSameOrderPolicyExist;
            }
        }
        
        
        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
        {
            return policy.ScopeType switch
            {
                MarkupPolicyScopeType.Agent when policy.AgentId.HasValue && policy.AgencyId.HasValue
                    => _displayedMarkupFormulaService.UpdateAgentFormula(policy.AgentId.Value, policy.AgencyId.Value),
                
                MarkupPolicyScopeType.Agency when policy.AgencyId.HasValue
                    => _displayedMarkupFormulaService.UpdateAgencyFormula(policy.AgencyId.Value),
                
                MarkupPolicyScopeType.Counterparty when policy.CounterpartyId.HasValue
                    => _displayedMarkupFormulaService.UpdateCounterpartyFormula(policy.CounterpartyId.Value),
                
                MarkupPolicyScopeType.Global
                    => _displayedMarkupFormulaService.UpdateGlobalFormula(),

                _ => Task.FromResult(Result.Success())
            };
        }


        private async Task WriteAuditLog(MarkupPolicy policy, MarkupPolicyEventOperationType type)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            
            var writeLogTask = (policy.ScopeType, type) switch
            {
                (MarkupPolicyScopeType.Agent, MarkupPolicyEventOperationType.Created) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupCreated),
                (MarkupPolicyScopeType.Agent, MarkupPolicyEventOperationType.Modified) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupUpdated),
                (MarkupPolicyScopeType.Agent, MarkupPolicyEventOperationType.Deleted) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupDeleted),
                (MarkupPolicyScopeType.Agency, MarkupPolicyEventOperationType.Created) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupCreated),
                (MarkupPolicyScopeType.Agency, MarkupPolicyEventOperationType.Modified) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupUpdated),
                (MarkupPolicyScopeType.Agency, MarkupPolicyEventOperationType.Deleted) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupDeleted),
                (MarkupPolicyScopeType.Counterparty, MarkupPolicyEventOperationType.Created) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupCreated),
                (MarkupPolicyScopeType.Counterparty, MarkupPolicyEventOperationType.Modified) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupUpdated),
                (MarkupPolicyScopeType.Counterparty, MarkupPolicyEventOperationType.Deleted) => WriteCounterpartyLog(MarkupPolicyEventType.CounterpartyMarkupDeleted),
                (MarkupPolicyScopeType.Global, MarkupPolicyEventOperationType.Created) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupCreated),
                (MarkupPolicyScopeType.Global, MarkupPolicyEventOperationType.Modified) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupUpdated),
                (MarkupPolicyScopeType.Global, MarkupPolicyEventOperationType.Deleted) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupDeleted),
                _ => Task.CompletedTask
            };

            await writeLogTask;
            
            
            Task WriteAgentLog(MarkupPolicyEventType eventType)
                => _markupPolicyAuditService.Write(eventType, new AgentMarkupPolicyData(policy.Id, policy.AgentId.Value, policy.AgencyId.Value), administrator.ToApiCaller());


            Task WriteAgencyLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new AgencyMarkupPolicyData(policy.Id, policy.AgencyId.Value), administrator.ToApiCaller());


            Task WriteCounterpartyLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new CounterpartyMarkupPolicyData(policy.Id, policy.CounterpartyId.Value), administrator.ToApiCaller());


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