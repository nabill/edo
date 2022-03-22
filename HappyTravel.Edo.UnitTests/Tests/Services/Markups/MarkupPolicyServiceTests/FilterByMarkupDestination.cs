using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups.Abstractions;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.MarkupPolicyServiceTests
{
    public class FilterByMarkupDestination
    {
        // [Fact]
        // public void Markups_for_hotel_country_should_be_returned()
        // {
        //     var markupSubject = GetDummyMarkupSubject();

        //     var markupDestination = new MarkupDestinationInfo
        //     {
        //         AccommodationHtId = "President Hotel", 
        //         CountryHtId = "UAE", 
        //         LocalityHtId = "Dubai"
        //     };

        //     var markupPolicies = new List<MarkupPolicy>
        //     {
        //         new()
        //         {
        //             Id = 1,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Country,
        //             DestinationScopeId = "UAE"
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Country,
        //             DestinationScopeId = "Russia"
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Single(policies);
        //     Assert.Equal(1, policies[0].Id);
        // }


        // [Fact]
        // public void Markups_for_hotel_locality_should_be_returned()
        // {
        //     var markupSubject = GetDummyMarkupSubject();

        //     var markupDestination = new MarkupDestinationInfo
        //     {
        //         AccommodationHtId = "President Hotel",
        //         CountryHtId = "UAE",
        //         LocalityHtId = "Dubai"
        //     };

        //     var markupPolicies = new List<MarkupPolicy>
        //     {
        //         new()
        //         {
        //             Id = 1,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Locality,
        //             DestinationScopeId = "Dubai"
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Locality,
        //             DestinationScopeId = "London"
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Single(policies);
        //     Assert.Equal(1, policies[0].Id);
        // }


        // [Fact]
        // public void Markups_for_specific_hotel_should_be_returned()
        // {
        //     var markupSubject = GetDummyMarkupSubject();

        //     var markupDestination = new MarkupDestinationInfo
        //     {
        //         AccommodationHtId = "President Hotel",
        //         CountryHtId = "UAE",
        //         LocalityHtId = "Dubai"
        //     };

        //     var markupPolicies = new List<MarkupPolicy>
        //     {
        //         new()
        //         {
        //             Id = 1,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
        //             DestinationScopeId = "President Hotel"
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
        //             DestinationScopeId = "Hilton"
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Single(policies);
        //     Assert.Equal(1, policies[0].Id);
        // }


        // private MarkupSubjectInfo GetDummyMarkupSubject()
        //     => new()
        //     {
        //         AgencyAncestors = new List<int>(),
        //         AgencyId = 1,
        //         AgentId = 1,
        //         CountryHtId = "Russia",
        //         LocalityHtId = "Moscow"
        //     };
    }
}