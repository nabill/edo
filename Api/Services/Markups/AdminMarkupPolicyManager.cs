using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Markups.Templates;
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
            INormalizationAgentMarkupService normalizationAgentMarkup)
        {
            _context = context;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;;
            _normalizationAgentMarkup = normalizationAgentMarkup;
        }
        
        
        public Task<Result> Add(MarkupPolicyData policyData)
        {
            return ValidatePolicy(policyData)
                .Map(SavePolicy)
                .Bind(UpdateNormalizedAgentMarkup);


            async Task<int?> SavePolicy()
            {
                var now = _dateTimeProvider.UtcNow();
                var (type, counterpartyId, agencyId, agentId) = policyData.Scope;

                var policy = new MarkupPolicy
                {
                    Description = policyData.Settings.Description,
                    Order = policyData.Settings.Order,
                    ScopeType = type,
                    Target = policyData.Target,
                    AgencyId = agencyId,
                    CounterpartyId = counterpartyId,
                    AgentId = agentId,
                    TemplateSettings = policyData.Settings.TemplateSettings,
                    Currency = policyData.Settings.Currency,
                    Created = now,
                    Modified = now,
                    TemplateId = policyData.Settings.TemplateId
                };

                _context.MarkupPolicies.Add(policy);
                await _context.SaveChangesAsync();
                return policy.AgentId;
            }
        }


        public async Task<Result> Remove(int policyId)
        {
            return await GetPolicy()
                .Map(DeletePolicy)
                .Bind(UpdateNormalizedAgentMarkup);


            async Task<Result<MarkupPolicy>> GetPolicy()
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
                return policy == null
                    ? Result.Failure<MarkupPolicy>("Could not find policy")
                    : Result.Success(policy);
            }


            async Task<int?> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return policy.AgentId;
            }
        }


        public async Task<Result> Modify(int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure("Could not find policy");

            return await Result.Success()
                .Bind(UpdatePolicy)
                .Bind(UpdateNormalizedAgentMarkup);


            async Task<Result<int?>> UpdatePolicy()
            {
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();

                var (_, isFailure, error) = await ValidatePolicy(GetPolicyData(policy));
                if (isFailure)
                    return Result.Failure<int?>(error);

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return policy.AgentId;
            }
        }


        public async Task<List<MarkupPolicyData>> Get(MarkupPolicyScope scope)
        {
            return (await GetPoliciesForScope(scope))
                .Select(GetPolicyData)
                .ToList();
        }


        private Task<List<MarkupPolicy>> GetPoliciesForScope(MarkupPolicyScope scope)
        {
            var (type, counterpartyId, agencyId, agentId) = scope;
            return type switch
            {
                MarkupPolicyScopeType.Global => _context.MarkupPolicies.Where(p => p.ScopeType == MarkupPolicyScopeType.Global).ToListAsync(),
                MarkupPolicyScopeType.Counterparty => _context.MarkupPolicies
                    .Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.CounterpartyId == counterpartyId)
                    .ToListAsync(),
                MarkupPolicyScopeType.Agency => _context.MarkupPolicies.Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.AgencyId == agencyId)
                    .ToListAsync(),
                MarkupPolicyScopeType.Agent => _context.MarkupPolicies.Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.AgentId == agentId)
                    .ToListAsync(),
                _ => Task.FromResult(new List<MarkupPolicy>(0))
            };
        }


        private static MarkupPolicyData GetPolicyData(MarkupPolicy policy)
        {
            return new MarkupPolicyData(policy.Target,
                new MarkupPolicySettings(policy.Description, policy.TemplateId, policy.TemplateSettings, policy.Order, policy.Currency),
                GetPolicyScope());


            MarkupPolicyScope GetPolicyScope()
            {
                // Policy can belong to counterparty, agency or agent.
                var scopeId = policy.CounterpartyId ?? policy.AgencyId ?? policy.AgentId;
                return new MarkupPolicyScope(policy.ScopeType, scopeId);
            }
        }


        private Task<Result> ValidatePolicy(MarkupPolicyData policyData)
        {
            return ValidateTemplate()
                .Ensure(ScopeIsValid, "Invalid scope data")
                .Ensure(TargetIsValid, "Invalid policy target")
                .Ensure(PolicyOrderIsUniqueForScope, "Policy with same order is already defined");


            Result ValidateTemplate() => _templateService.Validate(policyData.Settings.TemplateId, policyData.Settings.TemplateSettings);


            bool ScopeIsValid()
            {
                var (type, counterpartyId, _, _) = policyData.Scope;
                return type switch
                {
                    MarkupPolicyScopeType.Global => counterpartyId == null,
                    MarkupPolicyScopeType.Counterparty => counterpartyId != null,
                    MarkupPolicyScopeType.Agency => counterpartyId != null,
                    MarkupPolicyScopeType.Agent => counterpartyId != null,
                    MarkupPolicyScopeType.EndClient => counterpartyId != null,
                    _ => false
                };
            }


            bool TargetIsValid() => policyData.Target != MarkupPolicyTarget.NotSpecified;


            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                var isSameOrderPolicyExist = (await GetPoliciesForScope(policyData.Scope))
                    .Any(p => p.Order == policyData.Settings.Order);

                return !isSameOrderPolicyExist;
            }
        }
        
        
        private Task<Result> UpdateNormalizedAgentMarkup(int? agentId)
        {
            return agentId.HasValue
                ? _normalizationAgentMarkup.UpdateMarkup(agentId.Value)
                : Task.FromResult(Result.Success());
        }
        

        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INormalizationAgentMarkupService _normalizationAgentMarkup;
    }
}