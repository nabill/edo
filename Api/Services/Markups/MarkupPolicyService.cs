using System;
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
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);
            
            var policies = _markupPolicyStorage.Get(policy =>
                IsApplicableBySubject(policy, subjectInfo) && IsApplicableByObject(policy, objectInfo)
            );

            return policies
                .OrderBy(p => p.AgentScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.AgentScopeType == AgentMarkupScopeTypes.Agency && p.AgentScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.AgentScopeId)) : 0)
                .ThenBy(p => p.Order)
                .ToList();
            
            static bool IsApplicableBySubject(MarkupPolicy policy, MarkupSubjectInfo info) => policy.AgentScopeType switch
            {
                AgentMarkupScopeTypes.Global => true,
                AgentMarkupScopeTypes.Country => policy.AgentScopeId == info.CountryHtId,
                AgentMarkupScopeTypes.Locality => policy.AgentScopeId == info.LocalityHtId,
                AgentMarkupScopeTypes.Counterparty => policy.AgentScopeId == info.CounterpartyId.ToString(),
                AgentMarkupScopeTypes.Agency => policy.AgentScopeId == info.AgencyId.ToString()
                    || info.AgencyAncestors.Contains(int.Parse(policy.AgentScopeId)),
                AgentMarkupScopeTypes.Agent => policy.AgentScopeId == AgentInAgencyId.Create(info.AgentId, info.AgencyId).ToString(),
                _ => throw new ArgumentOutOfRangeException()
            };


            static bool IsApplicableByObject(MarkupPolicy policy, MarkupObjectInfo info)
            {
                var destinationScopeId = policy.DestinationScopeId;
                return string.IsNullOrWhiteSpace(destinationScopeId) || destinationScopeId == info.CountryHtId
                    || destinationScopeId == info.LocalityHtId || destinationScopeId == info.AccommodationHtId;
            }
        }

        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}