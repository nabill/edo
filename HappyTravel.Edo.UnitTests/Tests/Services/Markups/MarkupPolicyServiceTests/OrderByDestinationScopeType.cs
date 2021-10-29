using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class OrderByDestinationScopeType
    {
        [Fact]
        public void Ordering_by_agent_scope_type()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>(),
                AgencyId = 1, 
                AgentId = 1, 
                CounterpartyId = 1, 
                CountryHtId = "Russia",
                LocalityHtId = "Moscow"
            };

            var markupObject = new MarkupObjectInfo
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
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UAE"
                },
                new()
                {
                    Id = 7,
                    Order = 4,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 5,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel"
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Locality,
                    DestinationScopeId = "Dubai"
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Equal(DestinationMarkupScopeTypes.Global, policies[0].DestinationScopeType);
            Assert.Equal(DestinationMarkupScopeTypes.Country, policies[1].DestinationScopeType);
            Assert.Equal(DestinationMarkupScopeTypes.Locality, policies[2].DestinationScopeType);
            Assert.Equal(DestinationMarkupScopeTypes.Accommodation, policies[3].DestinationScopeType);
        }
    }
}