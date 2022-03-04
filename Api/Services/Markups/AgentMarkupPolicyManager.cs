using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agent;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
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


        public Task<Result> Set(int agentId, SetAgentMarkupRequest request, AgentContext agent)
        {
            // TODO: Remove after templates removal
            var settings = new MarkupPolicySettings(string.Empty, 1, new Dictionary<string, decimal>()
            {
                { "factor", (100 + request.Percent / 100) }
            }, Currencies.USD);
            var agentInAgencyId = AgentInAgencyId.Create(agentId, agent.AgencyId);
            
            return ValidateSettings(settings)
                .Map(() => GetAgentPolicy(agentInAgencyId))
                .Bind(SavePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);

            
            Result ValidateSettings(MarkupPolicySettings settings) 
                => _templateService.Validate(settings.TemplateId, settings.TemplateSettings);


            async Task<Result<MarkupPolicy>> SavePolicy(MarkupPolicy policy)
            {
                var (_, isFailure, destinationScopeType, error) = await GetDestinationScopeType(settings.DestinationScopeId);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);
                
                var now = _dateTimeProvider.UtcNow();

                if (policy is null)
                {
                    policy = new MarkupPolicy
                    {
                        Description = settings.Description,
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
                }
                
                MarkupPolicyValueUpdater.FillValuesFromTemplateSettings(policy, settings.TemplateSettings);
                _context.MarkupPolicies.Update(policy);
                
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgentMarkupUpdated,
                    new AgentMarkupPolicyData(policy.Id, agentId, agent.AgencyId),
                    agent.ToApiCaller());
        }


        public Task<Result> Remove(int agentId, AgentContext agent)
        {
            return Result.Success(AgentInAgencyId.Create(agentId, agent.AgencyId)) 
                .Map(GetPolicy)
                .Map(DeletePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            Task<MarkupPolicy> GetPolicy(AgentInAgencyId agentId) 
                => GetAgentPolicy(agentId);


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


        public async Task<AgentMarkupInfo?> Get(int agentId, AgentContext agent)
        {
            var agentInAgencyId = AgentInAgencyId.Create(agentId, agent.AgencyId);
            var policy = await GetAgentPolicy(agentInAgencyId);
            return policy is not null
                ? new AgentMarkupInfo { Percent = policy.Value }
                : null;
        }


        private Task<MarkupPolicy> GetAgentPolicy(AgentInAgencyId agentId)
        {
            var agentInAgencyId = agentId.ToString();
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agent && p.SubjectScopeId == agentInAgencyId)
                .SingleOrDefaultAsync();
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