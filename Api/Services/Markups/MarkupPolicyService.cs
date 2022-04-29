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


        public List<MarkupPolicy> Get(MarkupSubjectInfo subjectInfo, MarkupDestinationInfo destinationInfo)
        {
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);

            var policies = _markupPolicyStorage.Get(policy =>
                IsApplicableBySubject(policy, subjectInfo) && IsApplicableByObject(policy, destinationInfo)
            );

            return policies
                .OrderBy(p => p.FunctionType)
                .ThenBy(p => p.SubjectScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.SubjectScopeId)) : 0)
                .ToList();

            static bool IsApplicableBySubject(MarkupPolicy policy, MarkupSubjectInfo info) => policy.SubjectScopeType switch
            {
                // This code will be uncommented at the second stage of work on markups - Issue - AA #1310
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


            static bool IsApplicableByObject(MarkupPolicy policy, MarkupDestinationInfo info)
            {
                var destinationScopeId = policy.DestinationScopeId;
                return string.IsNullOrWhiteSpace(destinationScopeId) || destinationScopeId == info.MarketId.ToString() ||
                    destinationScopeId == info.CountryCode; // || destinationScopeId == info.LocalityHtId || destinationScopeId == info.AccommodationHtId;*/
            }
        }


        public List<MarkupPolicy> GetSecondLevel(MarkupSubjectInfo subjectInfo, MarkupDestinationInfo destinationInfo)
        {
            var agencyId = subjectInfo.AgencyId;
            var agencyTreeIds = subjectInfo.AgencyAncestors;
            agencyTreeIds.Add(agencyId);

            var policies = _markupPolicyStorage.Get(policy =>
                IsApplicableBySubject(policy, subjectInfo) &&
                IsApplicableByObject(policy, destinationInfo) &&
                IsApplicableBySupplier(policy, destinationInfo)
            );

            return policies
                .OrderBy(p => p.FunctionType)
                .ThenBy(p => p.SubjectScopeType)
                .ThenBy(p => p.DestinationScopeType)
                .ThenBy(p => p.SubjectScopeType == SubjectMarkupScopeTypes.Agency && p.SubjectScopeId != string.Empty ? agencyTreeIds.IndexOf(int.Parse(p.SubjectScopeId)) : 0)
                .ToList();

            static bool IsApplicableBySubject(MarkupPolicy policy, MarkupSubjectInfo info) => policy.SubjectScopeType switch
            {
                // This code will be uncommented at the second stage of work on markups - Issue - AA #1310
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


            static bool IsApplicableByObject(MarkupPolicy policy, MarkupDestinationInfo info)
            {
                var destinationScopeId = policy.DestinationScopeId;
                return string.IsNullOrWhiteSpace(destinationScopeId) || destinationScopeId == info.MarketId.ToString() ||
                    destinationScopeId == info.CountryCode; // || destinationScopeId == info.LocalityHtId || destinationScopeId == info.AccommodationHtId;*/
            }


            static bool IsApplicableBySupplier(MarkupPolicy policy, MarkupDestinationInfo info)
                => policy.SupplierCode == info.SupplierCode;
        }


        private readonly IMarkupPolicyStorage _markupPolicyStorage;
    }
}