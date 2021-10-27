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
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
                new()
                {
                    Id = 7,
                    Order = 4,
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
                    Id = 5,
                    Order = 1,
                    Target = default,
                    AgentScopeType = AgentMarkupScopeTypes.Agency,
                    AgentScopeId = "3",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global,
                    DestinationScopeId = null,
                    TemplateId = default,
                    TemplateSettings = default
                },
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);
            
            var policies = service.Get(markupSubject, markupObject, default);
            
            Assert.Equal("3", policies[0].AgentScopeId);
            Assert.Equal("2", policies[1].AgentScopeId);
            Assert.Equal("1", policies[2].AgentScopeId);
        }
    }
}