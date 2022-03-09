using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Markups.Global;
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
using HappyTravel.Money.Enums;
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
        

        public async Task<GlobalMarkupInfo?> GetGlobalPolicy()
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global)
                .SingleOrDefaultAsync();

            return policy is null
                ? null
                : new GlobalMarkupInfo { Percent = policy.Value };
        }


        public async Task<Result> RemoveGlobalPolicy()
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global);

            return policy is null
                ? Result.Failure("Could not find global policy")
                : await Remove(policy.Id);
        }


        public async Task<Result> SetGlobalPolicy(SetGlobalMarkupRequest request)
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Global);

            var settings = new MarkupPolicySettings("Global markup", MarkupFunctionType.Percent,
                request.Percent, Currencies.USD);

            if (settings.Value <= 0)
                return Result.Failure("Global markup policy must have positive value");
            
            if (policy is null)
            {
                var policyData = new MarkupPolicyData(settings,
                    new MarkupPolicyScope(SubjectMarkupScopeTypes.Global));
                
                return await Add(policyData);
            }

            return await Modify(policy.Id, settings);
        }


        public Task<List<MarkupInfo>> GetLocationPolicies()
        {
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Country || p.SubjectScopeType == SubjectMarkupScopeTypes.Locality)
                .OrderBy(p => p.FunctionType)
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToListAsync();
        }

        
        public async Task<Result> AddLocationPolicy(MarkupPolicySettings settings)
        {
            var (_, isFailure, agentMarkupScopeType, error) = await GetAgentMarkupScopeType(settings.LocationScopeId);
            if (isFailure)
                return Result.Failure(error);
                
            return await Add(new MarkupPolicyData(settings,
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


        public async Task<Result> SetAgencyPolicy(int agencyId, SetAgencyMarkupRequest request)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency)
                .Where(p => p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();

            var settings = new MarkupPolicySettings("Global markup", MarkupFunctionType.Percent,
                request.Percent, Currencies.USD);
            
            // TODO: Add a validation to not allow negative markup
            // https://github.com/happy-travel/agent-app-project/issues/1244
            if (policy is null)
            {
                var policyData = new MarkupPolicyData(settings,
                    new MarkupPolicyScope(SubjectMarkupScopeTypes.Agency, agencyId));
                
                return await Add(policyData);
            }

            return await Modify(policy.Id, settings);
        }
            

        public async Task<AgencyMarkupInfo?> GetForAgency(int agencyId)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();

            return policy is null
                ? null
                : new AgencyMarkupInfo { Percent = policy.Value };
        }


        public async Task<Result> RemoveAgencyPolicy(int agencyId)
        {
            var policy = await _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .SingleOrDefaultAsync();
            
            return policy is null
                ? Result.Failure("Could not find agency policy")
                : await Remove(policy.Id);;
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


        private static Result<MarkupPolicyData> GetPolicyData(MarkupPolicy policy)
        {
            int? agencyId = null, agentId = null;
            string? locationId = null;

            if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agency && int.TryParse(policy.SubjectScopeId, out var parsedId))
                agencyId = parsedId;

            if (policy.SubjectScopeType == SubjectMarkupScopeTypes.Agent)
            {
                var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
                agencyId = agentInAgencyId.AgencyId;
                agentId = agentInAgencyId.AgentId;
            }

            if (policy.SubjectScopeType is SubjectMarkupScopeTypes.Locality or SubjectMarkupScopeTypes.Country)
                locationId = policy.SubjectScopeId;

            return new MarkupPolicyData(new MarkupPolicySettings(policy.Description, policy.FunctionType, policy.Value, policy.Currency, policy.DestinationScopeId),
                new MarkupPolicyScope(policy.SubjectScopeType, agencyId, agentId, locationId));
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
        
        
        private async Task<Result> Add(MarkupPolicyData policyData)
        {
            var destinationScopeType = await GetDestinationScopeType(policyData.Settings.DestinationScopeId);
            if (destinationScopeType.IsFailure)
                return Result.Failure(destinationScopeType.Error);
            
            var (_, isFailure, markupPolicy, error) = await Result.Success()
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
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    SubjectScopeType = type,
                    SubjectScopeId = agentScopeId,
                    DestinationScopeId = settings.DestinationScopeId,
                    DestinationScopeType = destinationScopeType.Value,
                    FunctionType = MarkupFunctionType.Percent,
                    Value = settings.Value
                };

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }


        private async Task<Result> Remove(int policyId)
        {
            var (_, isFailure, markupPolicy, error) = await GetPolicy()
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


            async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
        }

        private async Task<Result> Modify(int policyId, MarkupPolicySettings settings)
        {
            var destinationScopeType = await GetDestinationScopeType(settings.DestinationScopeId);
            if (destinationScopeType.IsFailure)
                return Result.Failure(destinationScopeType.Error);
            
            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure("Could not find policy");

            var (_, isFailure, markupPolicy, error) = await UpdatePolicy()
                .Tap(p => WriteAuditLog(p, MarkupPolicyEventOperationType.Modified));

            if (isFailure)
                return Result.Failure(error);

            return await UpdateDisplayedMarkupFormula(markupPolicy);

            async Task<Result<MarkupPolicy>> UpdatePolicy()
            {
                policy.Description = settings.Description;
                policy.FunctionType = MarkupFunctionType.Percent;
                policy.Value = settings.Value;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();
                policy.SubjectScopeId = settings.LocationScopeId;
                // No SubjectScopeType here because changing its type is not allowed
                policy.DestinationScopeId = settings.DestinationScopeId;
                policy.DestinationScopeType = destinationScopeType.Value;

                var policyData = GetPolicyData(policy);
                if (policyData.IsFailure)
                    return Result.Failure<MarkupPolicy>(policyData.Error);

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return policy;
            }
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