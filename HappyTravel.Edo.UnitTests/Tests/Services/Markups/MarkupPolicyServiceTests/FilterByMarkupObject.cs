using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class FilterByMarkupObject
    {
        [Fact]
        public void Markups_for_hotel_country_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    AgentScopeId = null,
                    DestinationScopeType = DestinationMarkupScopeTypes.Country,
                    DestinationScopeId = "Russia",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };
            
            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("UAE", policies[0].DestinationScopeId);
        }


        [Fact]
        public void Markups_for_hotel_locality_should_be_returned()
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
                    DestinationScopeType = DestinationMarkupScopeTypes.Locality,
                    DestinationScopeId = "Dubai",
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 2,
                    Order = 2,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    AgentScopeId = null,
                    DestinationScopeType = DestinationMarkupScopeTypes.Locality,
                    DestinationScopeId = "London",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("Dubai", policies[0].DestinationScopeId);
        }

        
        [Fact]
        public void Markups_for_specific_hotel_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Global,
                    AgentScopeId = null,
                    DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
                    DestinationScopeId = "Hilton",
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("President Hotel", policies[0].DestinationScopeId);
        }
   }
}