using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
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
    public class ChildAgencyMarkupPolicyManager : IChildAgencyMarkupPolicyManager
    {
        public ChildAgencyMarkupPolicyManager(EdoContext context,
            IMarkupPolicyTemplateService templateService,
            IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService,
            IMarkupPolicyAuditService markupPolicyAuditService,
            IAccommodationMapperClient mapperClient)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _markupPolicyAuditService = markupPolicyAuditService;
            _mapperClient = mapperClient;
        }


        public Task<Result> Set(int agencyId, SetAgencyMarkupRequest request, AgentContext agent)
        {
            var settings = new MarkupPolicySettings(string.Empty, MarkupFunctionType.Percent, request.Percent, Currencies.USD);
            
            return ValidateSettings(settings)
                .Bind(() => GetForChildAgency(agencyId, agent))
                .Bind(SavePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


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
                        DestinationScopeType = destinationScopeType,
                        DestinationScopeId = settings.DestinationScopeId,
                        SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                        SubjectScopeId = agencyId.ToString(),
                        Currency = settings.Currency,
                        Created = now,
                        Modified = now,
                    };
                    _context.MarkupPolicies.Add(policy);
                }

                MarkupPolicyValueUpdater.FillValuesFromTemplateSettings(policy, MarkupFunctionType.Percent, request.Percent);

                _context.MarkupPolicies.Update(policy);
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupCreated,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)),
                    agent.ToApiCaller());
        }


        public Task<Result> Remove(int agencyId, AgentContext agent)
        {
            return GetForChildAgency(agencyId, agent)
                .Map(DeletePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            async Task<MarkupPolicy> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupDeleted,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)),
                    agent.ToApiCaller());
        }



        public async Task<Result<AgencyMarkupInfo?>> Get(int agencyId, AgentContext agentContext)
        {
            var (_, isFailure, markupPolicy, error) = await GetForChildAgency(agencyId, agentContext);
            if (isFailure)
                return Result.Failure<AgencyMarkupInfo?>(error);

            return new AgencyMarkupInfo { Percent = markupPolicy.Value };
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


        private Result ValidateSettings(MarkupPolicySettings settings)
        {
            return _templateService.Validate(settings.FunctionType, settings.Value);
        }
        
        
        private Task<Result<MarkupPolicy>> GetForChildAgency(int agencyId, AgentContext agent)
        {
            return Result.Success(agencyId)
                .Ensure(IsSpecifiedAgencyChild, "Specified agency is not a child agency or does not exist.")
                .Map(Get);


            async Task<bool> IsSpecifiedAgencyChild(int childAgencyId)
                => await _context.Agencies.AnyAsync(a => a.Id == childAgencyId && a.ParentId == agent.AgencyId && a.IsActive);


            Task<MarkupPolicy> Get(int childAgencyId)
                => _context.MarkupPolicies
                    .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency)
                    .Where(p => p.SubjectScopeId == agencyId.ToString())
                    .SingleOrDefaultAsync();
        }

        
        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
            => _displayedMarkupFormulaService.UpdateAgencyFormula(int.Parse(policy.SubjectScopeId));


        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IMarkupPolicyAuditService _markupPolicyAuditService;
        private readonly IAccommodationMapperClient _mapperClient;
    }
}