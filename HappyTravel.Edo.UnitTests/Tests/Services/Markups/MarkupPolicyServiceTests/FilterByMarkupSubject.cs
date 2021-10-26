using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class FilterByMarkupSubject
    {
        [Fact]
        public void Markups_for_specific_agent_country_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Country,
                    AgentScopeId = "Ukraine",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };
            
            var service = CreateMarkupPolicyService(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Single(policies);
            Assert.Equal("Russia", policies[0].AgentScopeId);
        }


        [Fact]
        public void Markups_for_specific_agent_locality_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Locality,
                    AgentScopeId = "Moscow",
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
                    AgentScopeId = "Ufa",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = CreateMarkupPolicyService(markupPolicies);

            var policies = service.Get(markupSubject, markupObject, default);

            Assert.Single(policies);
            Assert.Equal("Moscow", policies[0].AgentScopeId);
        }

        
        [Fact]
        public void Markups_for_specific_agency_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "2",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = CreateMarkupPolicyService(markupPolicies);

            var policies = service.Get(markupSubject, markupObject, default);

            Assert.Single(policies);
            Assert.Equal("1", policies[0].AgentScopeId);
        }

        
        [Fact]
        public void Markups_for_specific_agent_should_be_returned()
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
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 2, agencyId: 1).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 3,
                    Order = 1,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agent,
                    AgentScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 2).ToString(),
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = CreateMarkupPolicyService(markupPolicies);

            var policies = service.Get(markupSubject, markupObject, default);

            Assert.Single(policies);
            Assert.Equal(AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(), policies[0].AgentScopeId);
        }
        
        
        [Fact]
        public void Markups_by_ancestors_should_be_returned()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>{2, 3},
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
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "2",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 3,
                    Order = 1,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "3",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 4,
                    Order = 1,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "4",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                }
            };

            var service = CreateMarkupPolicyService(markupPolicies);

            var policies = service.Get(markupSubject, markupObject, default);

            Assert.Equal(3, policies.Count);
            Assert.NotEqual(4, policies[0].Id);
            Assert.NotEqual(4, policies[1].Id);
            Assert.NotEqual(4, policies[2].Id);
        }
        
        
        private MarkupPolicyService CreateMarkupPolicyService(List<MarkupPolicy> markupPolicies)
        {
            var monitor = Mock.Of<IOptionsMonitor<MarkupPolicyStorageOptions>>(_ => _.CurrentValue ==
                new MarkupPolicyStorageOptions { Timeout = TimeSpan.FromMilliseconds(1) });
            var markupPolicyStorage = new MarkupPolicyStorage(monitor);
            markupPolicyStorage.Set(markupPolicies);
            return new MarkupPolicyService(markupPolicyStorage);
        }
    }
}