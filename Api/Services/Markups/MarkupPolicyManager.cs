using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups.Templates;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyManager : IMarkupPolicyManager
    {
        public MarkupPolicyManager(EdoContext context,
            IAgentContext agentContext,
            IAdministratorContext administratorContext,
            IMarkupPolicyTemplateService templateService,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _agentContext = agentContext;
            _administratorContext = administratorContext;
            _templateService = templateService;
            _dateTimeProvider = dateTimeProvider;
        }


        public Task<Result> Add(MarkupPolicyData policyData)
        {
            return ValidatePolicy(policyData)
                .Bind(CheckPermissions)
                .Bind(SavePolicy);

            Task<Result> CheckPermissions() => CheckUserManagePermissions(policyData.Scope);


            async Task<Result> SavePolicy()
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
                return Result.Ok();
            }
        }


        public Task<Result> Remove(int policyId)
        {
            return GetPolicy()
                .Bind(CheckPermissions)
                .Bind(DeletePolicy);


            async Task<Result<MarkupPolicy>> GetPolicy()
            {
                var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
                if (policy == null)
                    return Result.Failure<MarkupPolicy>("Could not find policy");

                return Result.Ok(policy);
            }


            async Task<Result<MarkupPolicy>> CheckPermissions(MarkupPolicy policy)
            {
                var scopeType = policy.ScopeType;
                var scope = new MarkupPolicyScope(scopeType,
                    policy.CounterpartyId ?? policy.AgencyId ?? policy.AgentId);

                var (_, isFailure, error) = await CheckUserManagePermissions(scope);
                if (isFailure)
                    return Result.Failure<MarkupPolicy>(error);

                return Result.Ok(policy);
            }


            async Task<Result> DeletePolicy(MarkupPolicy policy)
            {
                _context.Remove(policy);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }


        public async Task<Result> Modify(int policyId, MarkupPolicySettings settings)
        {
            var policy = await _context.MarkupPolicies.SingleOrDefaultAsync(p => p.Id == policyId);
            if (policy == null)
                return Result.Failure("Could not find policy");

            return await Result.Ok()
                .Bind(CheckPermissions)
                .Bind(UpdatePolicy);


            Task<Result> CheckPermissions()
            {
                var scopeData = new MarkupPolicyScope(policy.ScopeType,
                    policy.CounterpartyId ?? policy.AgencyId ?? policy.AgentId);

                return CheckUserManagePermissions(scopeData);
            }


            async Task<Result> UpdatePolicy()
            {
                policy.Description = settings.Description;
                policy.Order = settings.Order;
                policy.TemplateId = settings.TemplateId;
                policy.TemplateSettings = settings.TemplateSettings;
                policy.Currency = settings.Currency;
                policy.Modified = _dateTimeProvider.UtcNow();

                var validateResult = await ValidatePolicy(GetPolicyData(policy));
                if (validateResult.IsFailure)
                    return validateResult;

                _context.Update(policy);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
        }


        public async Task<Result<List<MarkupPolicyData>>> Get(MarkupPolicyScope scope)
        {
            var (_, isFailure, error) = await CheckUserManagePermissions(scope);
            if (isFailure)
                return Result.Failure<List<MarkupPolicyData>>(error);

            var policies = (await GetPoliciesForScope(scope))
                .Select(GetPolicyData)
                .ToList();

            return Result.Ok(policies);
        }


        private Task<List<MarkupPolicy>> GetPoliciesForScope(MarkupPolicyScope scope)
        {
            var (type, counterpartyId, agencyId, agentId) = scope;
            switch (type)
            {
                case MarkupPolicyScopeType.Global:
                {
                    return _context.MarkupPolicies
                        .Where(p => p.ScopeType == MarkupPolicyScopeType.Global)
                        .ToListAsync();
                }
                case MarkupPolicyScopeType.Counterparty:
                {
                    return _context.MarkupPolicies
                        .Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.CounterpartyId == counterpartyId)
                        .ToListAsync();
                }
                case MarkupPolicyScopeType.Agency:
                {
                    return _context.MarkupPolicies
                        .Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.AgencyId == agencyId)
                        .ToListAsync();
                }
                case MarkupPolicyScopeType.Agent:
                {
                    return _context.MarkupPolicies
                        .Where(p => p.ScopeType == MarkupPolicyScopeType.Counterparty && p.AgentId == agentId)
                        .ToListAsync();
                }
                default:
                {
                    return Task.FromResult(new List<MarkupPolicy>(0));
                }
            }
        }


        private async Task<Result> CheckUserManagePermissions(MarkupPolicyScope scope)
        {
            var hasAdminPermissions = await _administratorContext.HasPermission(AdministratorPermissions.MarkupManagement);
            if (hasAdminPermissions)
                return Result.Ok();

            var agent = await _agentContext.GetAgent();

            var (type, counterpartyId, agencyId, agentId) = scope;
            switch (type)
            {
                case MarkupPolicyScopeType.Agent:
                {
                    var isMasterAgentInUserCounterparty = agent.CounterpartyId == counterpartyId
                        && agent.IsMaster;

                    return isMasterAgentInUserCounterparty
                        ? Result.Ok()
                        : Result.Failure("Permission denied");
                }
                case MarkupPolicyScopeType.Agency:
                {
                    var agency = await _context.Agencies
                        .SingleOrDefaultAsync(a => a.Id == agencyId);

                    if (agency == null)
                        return Result.Failure("Could not find agency");

                    var isMasterAgent = agent.CounterpartyId == agency.CounterpartyId
                        && agent.IsMaster;

                    return isMasterAgent
                        ? Result.Ok()
                        : Result.Failure("Permission denied");
                }
                case MarkupPolicyScopeType.EndClient:
                {
                    return agent.AgentId == agentId
                        ? Result.Ok()
                        : Result.Failure("Permission denied");
                }
                default:
                    return Result.Failure("Permission denied");
            }
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
                var scope = policyData.Scope;
                switch (scope.Type)
                {
                    case MarkupPolicyScopeType.Global:
                        return scope.ScopeId == null;
                    case MarkupPolicyScopeType.Counterparty:
                    case MarkupPolicyScopeType.Agency:
                    case MarkupPolicyScopeType.Agent:
                    case MarkupPolicyScopeType.EndClient:
                        return scope.ScopeId != null;
                    default:
                        return false;
                }
            }


            bool TargetIsValid() => policyData.Target != MarkupPolicyTarget.NotSpecified;


            async Task<bool> PolicyOrderIsUniqueForScope()
            {
                var isSameOrderPolicyExist = (await GetPoliciesForScope(policyData.Scope))
                    .Any(p => p.Order == policyData.Settings.Order);

                return !isSameOrderPolicyExist;
            }
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly IMarkupPolicyTemplateService _templateService;
        private readonly EdoContext _context;
        private readonly IAgentContext _agentContext;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}