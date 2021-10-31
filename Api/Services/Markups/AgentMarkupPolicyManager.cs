using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
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
    public class AgentMarkupPolicyManager : IAgentMarkupPolicyManager
    {
        public AgentMarkupPolicyManager(EdoContext context, IMarkupPolicyTemplateService templateService, IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService, IMarkupPolicyAuditService markupPolicyAuditService,
            IAccommodationMapperClient mapperClient)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _markupPolicyAuditService = markupPolicyAuditService;
            _mapperClient = mapperClient;
        }


        public Task<Result> Add(int agentId, MarkupPolicySettings settings, AgentContext agent)
        {
            return ValidateSettings(agentId, agent.AgencyId, settings)
                .Bind(() => GetAgentAgencyRelation(agentId, agent.AgencyId))
                .Bind(SavePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            async Task<Result<MarkupPolicy>> SavePolicy(AgentAgencyRelation agentAgencyRelation)
            {
                var (_, isFailure, destinationScopeType, error) = await GetDestinationScopeType(settings.DestinationScopeId);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);
                
                var now = _dateTimeProvider.UtcNow();
                var agentInAgencyId = AgentInAgencyId.Create(agentAgencyRelation.AgentId, agentAgencyRelation.AgencyId);

                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    Order = settings.Order,
                    Target = MarkupPolicyTarget.AccommodationAvailability,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                    SubjectScopeId = agentInAgencyId.ToString(),
                    DestinationScopeType = destinationScopeType,
                    DestinationScopeId = settings.DestinationScopeId,
                    TemplateSettings = settings.TemplateSettings,
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    TemplateId = settings.TemplateId
                };

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgentMarkupCreated,
                    new AgentMarkupPolicyData(policy.Id, agentId, agent.AgencyId),
                    agent.ToApiCaller());
        }


        public Task<Result> Remove(int agentId, int policyId, AgentContext agent)
        {
            return GetAgentAgencyRelation(agentId, agent.AgencyId) 
                .Bind(GetPolicy)
                .Map(DeletePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            Task<Result<MarkupPolicy>> GetPolicy(AgentAgencyRelation relation) 
                => GetAgentPolicy(relation, policyId);


            async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgentMarkupDeleted,
                    new AgentMarkupPolicyData(policy.Id, agentId, agent.AgencyId),
                    agent.ToApiCaller());
        }


        public async Task<Result> Modify(int agentId, int policyId, MarkupPolicySettings settings, AgentContext agent)
        {
            return await GetAgentAgencyRelation(agentId, agent.AgencyId) 
                .Bind(GetPolicy)
                .Check(_ => ValidateSettings(agentId, agent.AgencyId, settings, policyId))
                .Check(UpdatePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            Task<Result<MarkupPolicy>> GetPolicy(AgentAgencyRelation relation) => GetAgentPolicy(relation, policyId);

            
            async Task<Result> UpdatePolicy(MarkupPolicy policy)
            {
                var (_, isFailure, destinationScopeType, error) = await GetDestinationScopeType(settings.DestinationScopeId);
                if (isFailure)
                    return Result.Failure(error);
                
                policy.DestinationScopeId = settings.DestinationScopeId;
                policy.DestinationScopeType = destinationScopeType;
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return Result.Success();
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgentMarkupUpdated,
                    new AgentMarkupPolicyData(policy.Id, agentId, agent.AgencyId),
                    agent.ToApiCaller());
        }


        public async Task<List<MarkupInfo>> Get(int agentId, int agencyId)
        {
            var policies = await GetAgentPolicies(agentId, agencyId);
            return policies
                .Select(p=> new MarkupInfo(p.Id, p.GetSettings()))
                .ToList();
        }


        private Task<List<MarkupPolicy>> GetAgentPolicies(int agentId, int agencyId)
        {
            var agentInAgencyId = AgentInAgencyId.Create(agentId: agentId, agencyId: agencyId).ToString();
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agent && p.SubjectScopeId == agentInAgencyId)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        
        private async Task<Result<AgentAgencyRelation>> GetAgentAgencyRelation(int agentId, int agencyId)
        {
            var relation = await _context.AgentAgencyRelations
                .SingleOrDefaultAsync(r => r.AgentId == agentId && r.AgencyId == agencyId);
            
            return relation ?? Result.Failure<AgentAgencyRelation>("Could not find this agent in your agency"); 
        }
        

        private Task<Result> ValidateSettings(int agentId, int agencyId, MarkupPolicySettings settings, int? policyId = null)
        {
            return ValidateTemplate()
                .Ensure(PolicyOrderIsUniqueForScope, "Policy with same order is already defined");


            Result ValidateTemplate() => _templateService.Validate(settings.TemplateId, settings.TemplateSettings);


            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                var isSameOrderPolicyExist = (await GetAgentPolicies(agentId, agencyId))
                    .Any(p => p.Order == settings.Order && p.Id != policyId);

                return !isSameOrderPolicyExist;
            }
        }


        private async Task<Result<MarkupPolicy>> GetAgentPolicy(AgentAgencyRelation relation, int policyId)
        {
            var agentInAgencyId = AgentInAgencyId.Create(agentId: relation.AgentId, agencyId: relation.AgencyId).ToString();
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.Id == policyId && p.SubjectScopeType == SubjectMarkupScopeTypes.Agent && p.SubjectScopeId == agentInAgencyId);

            return policy ?? Result.Failure<MarkupPolicy>("Could not find agent policy");
        }


        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
        {
            var agentInAgencyId = AgentInAgencyId.Create(policy.SubjectScopeId);
            return _displayedMarkupFormulaService.UpdateAgentFormula(agentInAgencyId.AgentId, agentInAgencyId.AgencyId);
        }
        
        
        // TODO Replace code duplication: https://github.com/happy-travel/agent-app-project/issues/777
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


        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IMarkupPolicyAuditService _markupPolicyAuditService;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}