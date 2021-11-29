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
                var (type, agencyId, agentId, agentScopeId) = policyData.Scope;
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
                    SubjectScopeType = type,
                    SubjectScopeId = agentScopeId,
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
                if (policy.SubjectScopeType != SubjectMarkupScopeTypes.Agency)
                    return Result.Success();

                var agencyId = int.Parse(policy.SubjectScopeId);
                
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
                policy.SubjectScopeId = settings.LocationScopeId;
                // No SubjectScopeType here because changing its type is not allowed
                policy.DestinationScopeId = settings.DestinationScopeId;
                policy.DestinationScopeType = destinationScopeType.Value;

                var policyData = GetPolicyData(policy);
                if(policyData.IsFailure)
                    return Result.Failure<MarkupPolicy>(policyData.Error);

                var (_, isFailure, error) = await ValidatePolicy(policyData.Value, policy);
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
                .Where(p => p.IsSuccess)
                .Select(p => p.Value)
                .ToList();
        }


        public Task<List<MarkupInfo>> GetGlobalPolicies()
        {
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global)
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }


        public Task<Result> AddGlobalPolicy(MarkupPolicySettings settings)
            => Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings, new MarkupPolicyScope(SubjectMarkupScopeTypes.Global)));


        public async Task<Result> RemoveGlobalPolicy(int policyId)
        {
            var isGlobalPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Global &&
                    p.Id == policyId);
            
            return isGlobalPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' not found or not global");
        }


        public async Task<Result> ModifyGlobalPolicy(int policyId, MarkupPolicySettings settings)
        {
            var isGlobalPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Global &&
                    p.Id == policyId);
            
            return isGlobalPolicy
                ? await Modify(policyId, settings)
                : Result.Failure($"Policy '{policyId}' not found or not global");
        }


        public Task<List<MarkupInfo>> GetLocationPolicies()
        {
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Country || p.SubjectScopeType == SubjectMarkupScopeTypes.Locality)
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

            if (policy.SubjectScopeType is not (SubjectMarkupScopeTypes.Country or SubjectMarkupScopeTypes.Locality))
                return Result.Failure($"Policy '{policyId}' not found or not local");
            
            var (_, isFailure, agentMarkupScopeType, error) = await GetAgentMarkupScopeType(settings.LocationScopeId);
            if (isFailure)
                return Result.Failure(error);
            
            if (policy.SubjectScopeType != agentMarkupScopeType)
                return Result.Failure($"It is not allowed change location to a new type");
            
            return await Modify(policyId, settings);
        }


        public Task<Result> AddAgencyPolicy(int agencyId, MarkupPolicySettings settings) 
            => Add(new MarkupPolicyData(MarkupPolicyTarget.AccommodationAvailability, settings, new MarkupPolicyScope(SubjectMarkupScopeTypes.Agency, agencyId)));


        public Task<List<MarkupInfo>> GetForAgency(int agencyId)
        {
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .OrderBy(p => p.Order)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }


        public async Task<Result> RemoveAgencyPolicy(int agencyId, int policyId)
        {
            var isAgencyPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Agency &&
                    p.SubjectScopeId == agencyId.ToString() &&
                    p.Id == policyId);
            
            return isAgencyPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' not found or not agency");
        }


        public async Task<Result> ModifyForAgency(int agencyId, int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies
                .FirstOrDefaultAsync(p => p.Id == policyId && 
                    p.SubjectScopeType == SubjectMarkupScopeTypes.Agency &&
                    p.SubjectScopeId == agencyId.ToString());

            return policy is null 
                ? Result.Failure($"Policy '{policyId}' not found") 
                : await Modify(policyId, settings);
        }


        public async Task<Result> RemoveLocationPolicy(int policyId)
        {
            var isLocationPolicy = await _context.MarkupPolicies
                .AnyAsync(p =>
                    (p.SubjectScopeType == SubjectMarkupScopeTypes.Country || p.SubjectScopeType == SubjectMarkupScopeTypes.Locality) &&
                    p.Id == policyId);
            
            return isLocationPolicy
                ? await Remove(policyId)
                : Result.Failure($"Policy '{policyId}' not found or not local");
        }


        private Task<List<MarkupPolicy>> GetPoliciesForScope(MarkupPolicyScope scope)
        {
            var (agentScopeType, agencyId, agentId, agentScopeId) = scope;
            return agentScopeType switch
            {
                SubjectMarkupScopeTypes.Global => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global)
                    .ToListAsync(),
                SubjectMarkupScopeTypes.Country => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Country)
                    .ToListAsync(),
                SubjectMarkupScopeTypes.Locality => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Locality)
                    .ToListAsync(),
                SubjectMarkupScopeTypes.Agency => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agentScopeId)
                    .ToListAsync(),
                SubjectMarkupScopeTypes.Agent => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agent && p.SubjectScopeId == agentScopeId)
                    .ToListAsync(),
                _ => Task.FromResult(new List<MarkupPolicy>(0))
            };
        }


        private static Result<MarkupPolicyData> GetPolicyData(MarkupPolicy policy)
        {
            int? agencyId = null, agentId = null;
            string? locationId = null;

            try
            {
                if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agency)
                    agencyId = int.Parse(policy.SubjectScopeId);

                if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agent)
                {
                    var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
                    agencyId = agentInAgencyId.AgencyId;
                    agentId = agentInAgencyId.AgentId;
                }

                if (policy.SubjectScopeType is SubjectMarkupScopeTypes.Locality or SubjectMarkupScopeTypes.Country)
                    locationId = policy.SubjectScopeId;

                return new MarkupPolicyData(policy.Target,
                    new MarkupPolicySettings(policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order, policy.Currency, policy.DestinationScopeId),
                    new MarkupPolicyScope(policy.SubjectScopeType, agencyId, agentId, locationId));
            }
            catch (Exception ex)
            {
                return Result.Failure<MarkupPolicyData>(ex.Message);
            }
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
        
        
        private async Task<Result<SubjectMarkupScopeTypes>> GetAgentMarkupScopeType(string locationId)
        {
            var (_, isFailure, value, error) = await _mapperClient.GetSlimLocationDescription(locationId);
            if (isFailure)
                return Result.Failure<SubjectMarkupScopeTypes>(error.Detail);
        
            return value.Type switch
            {
                MapperLocationTypes.Country => SubjectMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => SubjectMarkupScopeTypes.Locality,
                _ => Result.Failure<SubjectMarkupScopeTypes>($"Type {value.Type} is not suitable")
            };
        }
        
        
        private async Task<Result<DestinationMarkupScopeTypes>> GetDestinationScopeType(string destinationScopeId)
        {
            // If destinationScopeId is not provided, treat it as Global
            if (string.IsNullOrWhiteSpace(destinationScopeId))
                return DestinationMarkupScopeTypes.Global;

            var (_, isFailure, value, error) = await _mapperClient.GetSlimLocationDescription(destinationScopeId);
            if (isFailure)
                return Result.Failure<DestinationMarkupScopeTypes>(error.Detail);

            return value.Type switch
            {
                MapperLocationTypes.Country => DestinationMarkupScopeTypes.Country,
                MapperLocationTypes.Locality => DestinationMarkupScopeTypes.Locality,
                MapperLocationTypes.Accommodation => DestinationMarkupScopeTypes.Accommodation,
                _ => Result.Failure<DestinationMarkupScopeTypes>($"Type {value.Type} is not suitable")
            };
        }
        
        
        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
        {
            return policy.SubjectScopeType switch
            {
                SubjectMarkupScopeTypes.Agent
                    => _displayedMarkupFormulaService.UpdateAgentFormula(AgentInAgencyId.Create(policy.SubjectScopeId).AgentId, AgentInAgencyId.Create(policy.SubjectScopeId).AgencyId),
                
                SubjectMarkupScopeTypes.Agency
                    => _displayedMarkupFormulaService.UpdateAgencyFormula(int.Parse(policy.SubjectScopeId)),
                
                SubjectMarkupScopeTypes.Global
                    => _displayedMarkupFormulaService.UpdateGlobalFormula(),

                _ => Task.FromResult(Result.Success())
            };
        }


        private async Task WriteAuditLog(MarkupPolicy policy, MarkupPolicyEventOperationType type)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            
            var writeLogTask = (AgentScopeType: policy.SubjectScopeType, type) switch
            {
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Created) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupCreated),
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Modified) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupUpdated),
                (SubjectMarkupScopeTypes.Agent, MarkupPolicyEventOperationType.Deleted) => WriteAgentLog(MarkupPolicyEventType.AgentMarkupDeleted),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Created) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupCreated),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Modified) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupUpdated),
                (SubjectMarkupScopeTypes.Agency, MarkupPolicyEventOperationType.Deleted) => WriteAgencyLog(MarkupPolicyEventType.AgencyMarkupDeleted),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Created) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupCreated),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Modified) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupUpdated),
                (SubjectMarkupScopeTypes.Global, MarkupPolicyEventOperationType.Deleted) => WriteGlobalLog(MarkupPolicyEventType.GlobalMarkupDeleted),
                _ => Task.CompletedTask
            };

            await writeLogTask;
            
            
            Task WriteAgentLog(MarkupPolicyEventType eventType)
            {
                var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
                return _markupPolicyAuditService.Write(eventType, new AgentMarkupPolicyData(policy.Id, agentInAgencyId.AgentId, agentInAgencyId.AgencyId), administrator.ToApiCaller());
            }


            Task WriteAgencyLog(MarkupPolicyEventType eventType) 
                => _markupPolicyAuditService.Write(eventType, new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)), administrator.ToApiCaller());


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