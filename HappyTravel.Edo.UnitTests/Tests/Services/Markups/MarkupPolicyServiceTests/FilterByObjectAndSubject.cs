using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class FilterByObjectAndSubject
    {
        [Fact]
        public void Global_markups_should_be_returned()
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
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Locality,
                    AgentScopeId = "Kiev",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }
        
        
        [Fact]
        public void Specific_agency_country_and_hotel_country()
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
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UAE"
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Ukraine",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UK"
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }


        [Fact]
        public void Specific_agency_country_and_specific_hotel()
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
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel"
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Ukraine",
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "Hilton"
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }

        
        [Fact]
        public void Specific_agency_and_specific_hotel_country()
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
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UAE"
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UK"
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }
        
        
        [Fact]
        public void Specific_agent_and_specific_hotel()
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
                    Id = 1,
                    Order = 1,
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel"
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 2, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel"
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }
    }
}