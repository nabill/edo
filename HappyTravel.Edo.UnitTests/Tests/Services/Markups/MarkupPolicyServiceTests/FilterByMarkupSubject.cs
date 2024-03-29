using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class FilterByMarkupSubject
    {
        // This tests will be uncommented at the second stage of work on markups - Issue - AA #1310

        [Fact]
        public void Markups_for_specific_agent_country_should_be_returned()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>(),
                AgencyId = 1,
                AgentId = 1,
                CountryHtId = "Russia",
                LocalityHtId = "Moscow",
                CountryCode = "RU"
            };

            var markupDestination = GetDummyMarkupDestination();

            var markupPolicies = new List<MarkupPolicy>
            {
                new()
                {
                    Id = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Country,
                    SubjectScopeId = "RU",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Country,
                    SubjectScopeId = "UK",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);

            var policies = service.Get(markupSubject, markupDestination);

            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }


        // [Fact]
        // public void Markups_for_specific_agent_locality_should_be_returned()
        // {
        //     var markupSubject = new MarkupSubjectInfo
        //     {
        //         AgencyAncestors = new List<int>(),
        //         AgencyId = 1,
        //         AgentId = 1,
        //         CountryHtId = "Russia",
        //         LocalityHtId = "Moscow"
        //     };

        //     var markupDestination = GetDummyMarkupDestination();

        //     var markupPolicies = new List<MarkupPolicy>
        //     {
        //         new()
        //         {
        //             Id = 1,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Locality,
        //             SubjectScopeId = "Moscow",
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Locality,
        //             SubjectScopeId = "Ufa",
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Single(policies);
        //     Assert.Equal(1, policies[0].Id);
        // }


        [Fact]
        public void Markups_for_specific_agency_should_be_returned()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>(),
                AgencyId = 1,
                AgentId = 1,
                CountryHtId = "Russia",
                LocalityHtId = "Moscow"
            };

            var markupDestination = GetDummyMarkupDestination();

            var markupPolicies = new List<MarkupPolicy>
            {
                new()
                {
                    Id = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "2",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);

            var policies = service.Get(markupSubject, markupDestination);

            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }


        // [Fact]
        // public void Markups_for_specific_agent_should_be_returned()
        // {
        //     var markupSubject = new MarkupSubjectInfo
        //     {
        //         AgencyAncestors = new List<int>(),
        //         AgencyId = 1,
        //         AgentId = 1,
        //         CountryHtId = "Russia",
        //         LocalityHtId = "Moscow"
        //     };

        //     var markupDestination = GetDummyMarkupDestination();

        //     var markupPolicies = new List<MarkupPolicy>
        //     {
        //         new()
        //         {
        //             Id = 1,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Agent,
        //             SubjectScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 1).ToString(),
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Agent,
        //             SubjectScopeId = AgentInAgencyId.Create(agentId: 2, agencyId: 1).ToString(),
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global,
        //         },
        //         new()
        //         {
        //             Id = 3,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Agent,
        //             SubjectScopeId = AgentInAgencyId.Create(agentId: 1, agencyId: 2).ToString(),
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Single(policies);
        //     Assert.Equal(1, policies[0].Id);
        // }


        [Fact]
        public void Markups_by_ancestors_should_be_returned()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int> { 2, 3 },
                AgencyId = 1,
                AgentId = 1,
                CountryHtId = "Russia",
                LocalityHtId = "Moscow"
            };

            var markupDestination = GetDummyMarkupDestination();

            var markupPolicies = new List<MarkupPolicy>
            {
                new()
                {
                    Id = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "2",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 3,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "3",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 4,
                    SubjectScopeType = SubjectMarkupScopeTypes.Agency,
                    SubjectScopeId = "4",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);

            var policies = service.Get(markupSubject, markupDestination);

            Assert.Equal(3, policies.Count);
            Assert.NotEqual(4, policies[0].Id);
            Assert.NotEqual(4, policies[1].Id);
            Assert.NotEqual(4, policies[2].Id);
        }


        [Fact]
        public void Markups_for_specific_market_should_be_returned()
        {
            var markupSubject = new MarkupSubjectInfo
            {
                AgencyAncestors = new List<int>(),
                AgencyId = 1,
                AgentId = 1,
                CountryHtId = "Russia",
                LocalityHtId = "Moscow",
                CountryCode = "RU",
                MarketId = 8
            };

            var markupDestination = GetDummyMarkupDestination();

            var markupPolicies = new List<MarkupPolicy>
            {
                new()
                {
                    Id = 1,
                    SubjectScopeType = SubjectMarkupScopeTypes.Market,
                    SubjectScopeId = "8",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                },
                new()
                {
                    Id = 2,
                    SubjectScopeType = SubjectMarkupScopeTypes.Market,
                    SubjectScopeId = "1",
                    DestinationScopeType = DestinationMarkupScopeTypes.Global
                }
            };

            var service = MarkupPolicyServiceMock.Create(markupPolicies);

            var policies = service.Get(markupSubject, markupDestination);

            Assert.Single(policies);
            Assert.Equal(1, policies[0].Id);
        }


        private MarkupDestinationInfo GetDummyMarkupDestination()
            => new()
            {
                AccommodationHtId = "President Hotel",
                CountryHtId = "UAE",
                LocalityHtId = "Dubai",
                CountryCode = "AE"
            };
    }
}