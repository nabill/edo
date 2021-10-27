using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Microsoft.Extensions.Options;
using Moq;
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
                    Id = 7,
                    Order = 4,
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
                    Id = 5,
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
                    DestinationScopeType = DestinationMarkupScopeTypes.Locality,
                    DestinationScopeId = "Dubai",
                    TemplateId = default,
                    TemplateSettings = default
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