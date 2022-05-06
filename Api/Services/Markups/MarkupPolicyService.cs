using System;
using System.Collections.Generic;
using System.Linq;
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


        public List<MarkupPolicy> Get(MarkupSubjectInfo subjectInfo, MarkupDestinationInfo destinationInfo)
        {
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);

            var policies = _markupPolicyStorage.Get(policy =>
                IsApplicableByLocation(policy, subjectInfo) &&
                IsApplicableByDestination(policy, destinationInfo) &&
                IsNotApplicableBySupplier(policy, destinationInfo)
            );

            return policies
                .OrderBy(p => p.FunctionType)
                .ThenBy(p => p.SubjectScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.SubjectScopeId)) : 0)
                .ToList();


            static bool IsNotApplicableBySupplier(MarkupPolicy policy, MarkupDestinationInfo info)
                => string.IsNullOrWhiteSpace(policy.SupplierCode);
        }


        public List<MarkupPolicy> GetSecondLevel(MarkupSubjectInfo subjectInfo, MarkupDestinationInfo destinationInfo)
        {
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);

            var policies = _markupPolicyStorage.Get(policy =>
                IsApplicableByLocation(policy, subjectInfo) &&
                IsApplicableByDestination(policy, destinationInfo) &&
                IsApplicableBySupplier(policy, destinationInfo)
            );

            return policies
                .OrderBy(p => p.FunctionType)
                .ThenBy(p => p.SubjectScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.SubjectScopeId)) : 0)
                .ToList();


            static bool IsApplicableBySupplier(MarkupPolicy policy, MarkupDestinationInfo info)
                => policy.SupplierCode == info.SupplierCode;
        }


        private static bool IsApplicableByLocation(MarkupPolicy policy, MarkupSubjectInfo info) => policy.SubjectScopeType switch
        {
            SubjectMarkupScopeTypes.Global => true,
            SubjectMarkupScopeTypes.Market => policy.SubjectScopeId == info.MarketId.ToString(),
            SubjectMarkupScopeTypes.Country => policy.SubjectScopeId == info.CountryCode,
            SubjectMarkupScopeTypes.Locality => false, // policy.SubjectScopeId == info.LocalityHtId,
            SubjectMarkupScopeTypes.Agency => policy.SubjectScopeId == info.AgencyId.ToString()
                || info.AgencyAncestors.Contains(int.Parse(policy.SubjectScopeId)),
            SubjectMarkupScopeTypes.Agent => false, // policy.SubjectScopeId == AgentInAgencyId.Create(info.AgentId, info.AgencyId).ToString(),
            SubjectMarkupScopeTypes.NotSpecified => false,
            _ => throw new ArgumentOutOfRangeException()
        };


        private static bool IsApplicableByDestination(MarkupPolicy policy, MarkupDestinationInfo info)
        {
            var destinationScopeId = policy.DestinationScopeId;
            return string.IsNullOrWhiteSpace(destinationScopeId) || destinationScopeId == info.MarketId.ToString() ||
                destinationScopeId == info.CountryCode; // || destinationScopeId == info.LocalityHtId || destinationScopeId == info.AccommodationHtId;*/
        }


        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}