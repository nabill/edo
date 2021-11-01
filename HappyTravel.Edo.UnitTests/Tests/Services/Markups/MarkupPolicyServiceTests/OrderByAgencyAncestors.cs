using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class OrderByAgencyAncestors
    {
        [Fact]
        public void Ordering_by_agency_ancestors()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>{3, 2},
                AgencyId = 1, 
                AgentId = 1, 
                CounterpartyId = 1, 
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
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 7,
                    Order = 4,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "2",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 5,
                    Order = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "3",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupDestination, default);
            
            Assert.Equal("3", policies[0].SubjectScopeId);
            Assert.Equal("2", policies[1].SubjectScopeId);
            Assert.Equal("1", policies[2].SubjectScopeId);
        }
    }
}