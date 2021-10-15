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
        public MarkupPolicyService(IMarkupPolicyStorage markupPolicyStorage)
        {
            _markupPolicyStorage = markupPolicyStorage;
        }


        public List<MarkupPolicy> Get(MarkupSubjectInfo subjectInfo, MarkupObjectInfo objectInfo, MarkupPolicyTarget policyTarget, List<int> agencyTreeIds)
        {
            var agentId = subjectInfo.AgentId;
            var counterpartyId = subjectInfo.CounterpartyId;
            var agencyId = subjectInfo.AgencyId;
            
            var policies = _markupPolicyStorage.Get(p =>
                    p.Target == policyTarget &&
                    p.ScopeType == MarkupPolicyScopeType.Global ||
                    p.ScopeType == MarkupPolicyScopeType.Counterparty && p.CounterpartyId == counterpartyId ||
                    p.ScopeType == MarkupPolicyScopeType.Agency && (p.AgencyId == agencyId || agencyTreeIds.Contains(p.AgencyId.Value)) ||
                    p.ScopeType == MarkupPolicyScopeType.Agent && p.AgentId == agentId && p.AgencyId == agencyId
                )
                .ToList();

            return policies
                .OrderBy(p => p.ScopeType)
                .ThenBy(p => p.ScopeType == MarkupPolicyScopeType.Agency && p.AgencyId.HasValue ? agencyTreeIds.IndexOf(p.AgencyId.Value) : 0)
                .ThenBy(p => p.Order)
                .ToList();
        }

        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}