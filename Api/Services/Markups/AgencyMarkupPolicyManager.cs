using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.AuditEvents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class AgencyMarkupPolicyManager : IAgencyMarkupPolicyManager
    {
        public AgencyMarkupPolicyManager(EdoContext context,
            IMarkupPolicyTemplateService templateService,
            IDateTimeProvider dateTimeProvider,
            IDisplayedMarkupFormulaService displayedMarkupFormulaService,
            IMarkupPolicyAuditService markupPolicyAuditService)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
            _displayedMarkupFormulaService = displayedMarkupFormulaService;
            _markupPolicyAuditService = markupPolicyAuditService;
        }
        
        
        public Task<Result> Add(int agencyId, MarkupPolicySettings settings, AgentContext agent)
        {
            return ValidateSettings(agencyId, settings)
                .Map(SavePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);


            async Task<MarkupPolicy> SavePolicy()
            {
                var now = _dateTimeProvider.UtcNow();

                var policy = new MarkupPolicy
                {
                    Description = settings.Description,
                    Order = settings.Order,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = agencyId.ToString(),
                    Target = MarkupPolicyTarget.AccommodationAvailability,
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
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupCreated,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.AgentScopeId)),
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
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.AgentScopeId)),
                    agent.ToApiCaller());
        }


        public Task<Result> Modify(int agencyId, int policyId, MarkupPolicySettings settings, AgentContext agent)
        {
            return GetAgencyPolicy(agencyId, policyId)
                .Check(_ => ValidateSettings(agencyId, settings, policyId))
                .Tap(UpdatePolicy)
                .Tap(WriteAuditLog)
                .Bind(UpdateDisplayedMarkupFormula);

            
            async Task UpdatePolicy(MarkupPolicy policy)
            {
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();

                _context.Update(policy);
                await _context.SaveChangesAsync();
            }


            Task WriteAuditLog(MarkupPolicy policy)
                => _markupPolicyAuditService.Write(MarkupPolicyEventType.AgencyMarkupUpdated,
                    new AgencyMarkupPolicyData(policy.Id, int.Parse(policy.AgentScopeId)),
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
                .Select(p=> new MarkupInfo(p.Id, p.GetSettings()))
                .ToList();
        }
        
        
        private Task<Result> ValidateSettings(int agencyId, MarkupPolicySettings settings, int? policyId = null)
        {
            return ValidateTemplate()
                .Ensure(PolicyOrderIsUniqueForScope, "Policy with same order is already defined");


            Result ValidateTemplate() => _templateService.Validate(settings.TemplateId, settings.TemplateSettings);


            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                var isSameOrderPolicyExist = (await GetAgencyPolicies(agencyId))
                    .Any(p => p.Order == settings.Order && p.Id != policyId);

                return !isSameOrderPolicyExist;
            }
        }
        
        
        private Task<List<MarkupPolicy>> GetAgencyPolicies(int agencyId)
        {
            return _context.MarkupPolicies
                .Where(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId == agencyId.ToString())
                .OrderBy(p => p.Order)
                .ToListAsync();
        }
        
        
        private async Task<Result<MarkupPolicy>> GetAgencyPolicy(int agencyId, int policyId)
        {
            var policy = await _context.MarkupPolicies
                .SingleOrDefaultAsync(p => p.Id == policyId && p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId == agencyId.ToString());

            return policy ?? Result.Failure<MarkupPolicy>("Could not find agency policy");
        }
        
        
        private Task<Result> UpdateDisplayedMarkupFormula(MarkupPolicy policy)
            => _displayedMarkupFormulaService.UpdateAgencyFormula(int.Parse(policy.AgentScopeId));
        
        
        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDisplayedMarkupFormulaService _displayedMarkupFormulaService;
        private readonly IMarkupPolicyAuditService _markupPolicyAuditService;
    }
}