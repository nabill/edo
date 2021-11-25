using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class OrderByAgentScopeType
    {
        [Fact]
        public void Ordering_by_subject_scope_type()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>(),
                AgencyId = 1, 
                AgentId = 1, 
                CountryHtId = "Russia",
                LocalityHtId = "Moscow"
            };

            var markupDestination = new MarkupDestinationInfo
            {
                AccommodationHtId = "President Hotel", 
                CountryHtId = "UAE", 
                LocalityHtId = "Dubai"
            };

            var markupPolicies = new List<MarkupPolicy>
            {
                new()
                {
                    Id = 9,
                    Order = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agent,
                    SubjectScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 7,
                    Order = 4,
                    SubjectScopeType = SubjectMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 5,
                    Order = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Country,
                    SubjectScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 8,
                    Order = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Locality,
                    SubjectScopeId = "Moscow",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupDestination, default);
            
            Assert.Equal(SubjectMarkupScopeTypes.Global, policies[0].SubjectScopeType);
            Assert.Equal(SubjectMarkupScopeTypes.Country, policies[1].SubjectScopeType);
            Assert.Equal(SubjectMarkupScopeTypes.Locality, policies[2].SubjectScopeType);
            Assert.Equal(SubjectMarkupScopeTypes.Agency, policies[3].SubjectScopeType);
            Assert.Equal(SubjectMarkupScopeTypes.Agent, policies[4].SubjectScopeType);
        }
    }
}