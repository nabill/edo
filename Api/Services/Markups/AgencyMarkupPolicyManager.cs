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
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.MapperContracts.Internal.Mappings.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class AgencyMarkupPolicyManager : IAgencyMarkupPolicyManager
    {
        public AgencyMarkupPolicyManager(EdoContext context,
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


        public Task<Result> Add(int agencyId, MarkupPolicySettings settings, AgentContext agent)
        {
            return ValidateSettings(settings)
                .Bind(SavePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            async Task<Result<MarkupPolicy>> SavePolicy()
            {
                var (_, isFailure, destinationScopeType, error) = await GetDestinationScopeType(settings.DestinationScopeId);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);
                
                var now = _dateTimeProvider.UtcNow();

                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    DestinationScopeType = destinationScopeType,
                    DestinationScopeId = settings.DestinationScopeId,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = agencyId.ToString(),
                    TemplateSettings = settings.TemplateSettings,
                    Currency = settings.Currency,
                    Created = now,
                    Modified = now,
                    TemplateId = settings.TemplateId
                };
                MarkupPolicyValueUpdater.FillValuesFromTemplateSettings(policy, settings.TemplateSettings);

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy;
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupCreated,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)),
                    agent.ToApiCaller());
        }


        public Task<Result> Remove(int agencyId, int policyId, AgentContext agent)
        {
            return GetAgencyPolicy(agencyId, policyId)
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


        public Task<Result> Modify(int agencyId, int policyId, MarkupPolicySettings settings, AgentContext agent)
        {
            return GetAgencyPolicy(agencyId, policyId)
                .Check(_ => ValidateSettings(settings))
                .Check(UpdatePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            async Task<Result> UpdatePolicy(MarkupPolicy policy)
            {
                var (_, isFailure, destinationScopeType, error) = await GetDestinationScopeType(settings.DestinationScopeId);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);

                policy.DestinationScopeId = settings.DestinationScopeId;
                policy.DestinationScopeType = destinationScopeType;
                policy.Description = settings.Description;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                MarkupPolicyValueUpdater.FillValuesFromTemplateSettings(policy, settings.TemplateSettings);
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return Result.Success();
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupUpdated,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.SubjectScopeId)),
                    agent.ToApiCaller());
        }


        public Task<Result<List<MarkupInfo>>> GetForChildAgency(int agencyId, AgentContext agent)
        {
            return Result.Success(agencyId)
                .Ensure(IsSpecifiedAgencyChild, "Specified agency is not a child agency or does not exist.")
                .Map(Get);


            async Task<bool> IsSpecifiedAgencyChild(int childAgencyId)
                => await _context.Agencies.AnyAsync(a => a.Id == childAgencyId && a.ParentId == agent.AgencyId && a.IsActive);
        }


        public async Task<List<MarkupInfo>> Get(int agencyId)
        {
            var policies = await GetAgencyPolicies(agencyId);
            return policies
                .Select(p => new MarkupInfo(p.Id, p.GetSettings()))
                .ToList();
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
            return _templateService.Validate(settings.TemplateId, settings.TemplateSettings);
        }


        private Task<List<MarkupPolicy>> GetAgencyPolicies(int agencyId)
        {
            return _context.MarkupPolicies
                .Where(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString())
                .ToListAsync();
        }


        private async Task<Result<MarkupPolicy>> GetAgencyPolicy(int agencyId, int policyId)
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.Id == policyId && p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId == agencyId.ToString());

            return policy ?? Result.Failure<MarkupPolicy>("Could not find agency policy");
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