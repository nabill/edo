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
                .OrderBy(p => p.SubjectScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.SubjectScopeId)) : 0)
                .ThenBy(p => p.Order)
                .ToList();
            
            static bool IsApplicableBySubject(MarkupPolicy policy, MarkupSubjectInfo info) => policy.SubjectScopeType switch
            {
                SubjectMarkupScopeTypes.Global => true,
                SubjectMarkupScopeTypes.Country => policy.SubjectScopeId == info.CountryHtId,
                SubjectMarkupScopeTypes.Locality => policy.SubjectScopeId == info.LocalityHtId,
                SubjectMarkupScopeTypes.Counterparty => policy.SubjectScopeId == info.CounterpartyId.ToString(),
                SubjectMarkupScopeTypes.Agency => policy.SubjectScopeId == info.AgencyId.ToString()
                    || info.AgencyAncestors.Contains(int.Parse(policy.SubjectScopeId)),
                SubjectMarkupScopeTypes.Agent => policy.SubjectScopeId == AgentInAgencyId.Create(info.AgentId, info.AgencyId).ToString(),
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