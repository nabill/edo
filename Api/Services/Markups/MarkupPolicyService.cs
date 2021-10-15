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


        public List<MarkupPolicy> Get(MarkupSubjectInfo subjectInfo, MarkupObjectInfo objectInfo, MarkupPolicyTarget policyTarget)
        {
            var agentId = subjectInfo.AgentId;
            var counterpartyId = subjectInfo.CounterpartyId;
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);
            
            var policies = _markupPolicyStorage.Get(p =>
                    p.Target == policyTarget &&
                    p.AgentScopeType == AgentMarkupScopeTypes.Global ||
                    p.AgentScopeType == AgentMarkupScopeTypes.Location && (p.AgentScopeId == subjectInfo.CountryHtId || p.AgentScopeId == subjectInfo.LocalityHtId) ||
                    p.AgentScopeType == AgentMarkupScopeTypes.Counterparty && p.AgentScopeId == counterpartyId.ToString() ||
                    p.AgentScopeType == AgentMarkupScopeTypes.Agency && (p.AgentScopeId == agencyId.ToString() || agencyTreeIds.Contains(int.Parse(p.AgentScopeId))) ||
                    p.AgentScopeType == AgentMarkupScopeTypes.Agent && p.AgentScopeId == $"{agencyId}-{agentId}"
                )
                .ToList();

            return policies
                .OrderBy(p => p.AgentScopeType)
                .ThenBy(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.AgentScopeId)) : 0)
                .ThenBy(p => p.Order)
                .ToList();
        }

        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}