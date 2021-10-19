using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;

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
            var agentInAgencyId = AgentInAgencyId.Create(agentId, agencyId).ToString();
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);
            
            var policies = _markupPolicyStorage.Get(p =>
                (p.DestinationScopeId == null || p.DestinationScopeId == objectInfo.LocalityHtId
                    || p.DestinationScopeId == objectInfo.CountryHtId || p.DestinationScopeId == objectInfo.AccommodationHtId)
                &&
                (p.AgentScopeType == AgentMarkupScopeTypes.Global
                    || p.AgentScopeType == AgentMarkupScopeTypes.Location && (p.AgentScopeId == subjectInfo.CountryHtId || p.AgentScopeId == subjectInfo.LocalityHtId)
                    || p.AgentScopeType == AgentMarkupScopeTypes.Counterparty && p.AgentScopeId == counterpartyId.ToString()
                    || p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId == subjectInfo.AgencyId.ToString()
                    || p.AgentScopeType == AgentMarkupScopeTypes.Agency && agencyTreeIds.Contains(int.Parse(p.AgentScopeId))
                    || p.AgentScopeType == AgentMarkupScopeTypes.Agent && p.AgentScopeId == $"{agentId}-{agencyId}")
            );

            return policies
                .OrderBy(p => p.AgentScopeType)
                .ThenBy(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.AgentScopeId)) : 0)
                .ThenBy(p => p.Order)
                .ToList();
        }

        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}