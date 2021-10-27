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
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    AgentScopeId = null,
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Locality,
                    AgentScopeId = "Kiev",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(AgentMarkupScopeTypes.Global, policies[0].AgentScopeType);
            Assert.Equal(DestinationMarkupScopeTypes.Global, policies[0].DestinationScopeType);
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
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UAE",
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Ukraine",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UK",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("Russia", policies[0].AgentScopeId);
            Assert.Equal("UAE", policies[0].DestinationScopeId);
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
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Russia",
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel",
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Ukraine",
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "Hilton",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("Russia", policies[0].AgentScopeId);
            Assert.Equal("President Hotel", policies[0].DestinationScopeId);
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
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UAE",
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "UK",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("1", policies[0].AgentScopeId);
            Assert.Equal("UAE", policies[0].DestinationScopeId);
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
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel",
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 2, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "President Hotel",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal(AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(), policies[0].AgentScopeId);
            Assert.Equal("President Hotel", policies[0].DestinationScopeId);
        }
    }
}