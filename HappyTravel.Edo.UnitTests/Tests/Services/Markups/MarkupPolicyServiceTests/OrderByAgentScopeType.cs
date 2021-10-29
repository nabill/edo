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
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
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
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Counterparty,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = "UAE"
                },
                new()
                {
                    Id = 8,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Locality,
                    AgentScopeId = "Moscow",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Equal(AgentMarkupScopeTypes.Global, policies[0].AgentScopeType);
            Assert.Equal(AgentMarkupScopeTypes.Country, policies[1].AgentScopeType);
            Assert.Equal(AgentMarkupScopeTypes.Locality, policies[2].AgentScopeType);
            Assert.Equal(AgentMarkupScopeTypes.Counterparty, policies[3].AgentScopeType);
            Assert.Equal(AgentMarkupScopeTypes.Agency, policies[4].AgentScopeType);
            Assert.Equal(AgentMarkupScopeTypes.Agent, policies[5].AgentScopeType);
        }
    }
}