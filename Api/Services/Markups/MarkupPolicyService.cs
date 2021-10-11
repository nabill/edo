using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Markup;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public class MarkupPolicyService : IMarkupPolicyService
    {
        public MarkupPolicyService(EdoContext context, IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public async Task<List<MarkupPolicy>> Get(MarkupSubjectInfo subject, MarkupPolicyTarget policyTarget)
        {
            return await GetAgentPolicies(subject, policyTarget);
        }


        private Task<List<MarkupPolicy>> GetAgentPolicies(MarkupSubjectInfo subject, MarkupPolicyTarget policyTarget)
        {
            var agentId = subject.AgentId;
            var counterpartyId = subject.CounterpartyId;
            var agencyId = subject.AgencyId;

            return _flow.GetOrSetAsync(BuildKey(),
                GetOrderedPolicies,
                AgentPoliciesCachingTime);


            string BuildKey()
                => _flow.BuildKey(nameof(MarkupPolicyService),
                    nameof(GetAgentPolicies),
                    agentId.ToString());


            async Task<List<MarkupPolicy>> GetOrderedPolicies()
            {
                var agencyTreeIds = await _context.Agencies
                    .Where(a => a.Id == agencyId)
                    .Select(a => a.Ancestors)
                    .SingleOrDefaultAsync() ?? new List<int>();
                
                agencyTreeIds.Add(agencyId);
                
                var policies = await _context.MarkupPolicies
                    .Where(p => p.Target == policyTarget)
                    .Where(p =>
                        p.ScopeType == MarkupPolicyScopeType.Global ||
                        p.ScopeType == MarkupPolicyScopeType.Counterparty && p.CounterpartyId == counterpartyId ||
                        p.ScopeType == MarkupPolicyScopeType.Agency && (p.AgencyId == agencyId || agencyTreeIds.Contains(p.AgencyId.Value)) ||
                        p.ScopeType == MarkupPolicyScopeType.Agent && p.AgentId == agentId && p.AgencyId == agencyId
                    )
                    .ToListAsync();

                return policies
                    .OrderBy(p => p.ScopeType)
                    .ThenBy(p => p.ScopeType == MarkupPolicyScopeType.Agency && p.AgencyId.HasValue ? agencyTreeIds.IndexOf(p.AgencyId.Value) : 0)
                    .ThenBy(p => p.Order)
                    .ToList();
            }
        }

        private static readonly TimeSpan AgentPoliciesCachingTime = TimeSpan.FromMinutes(2);
        private readonly EdoContext _context;
       
        private readonly IDoubleFlow _flow;
    }
}