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
        // This tests will be uncommented at the second stage of work on markups - Issue - AA #1310

        // [Fact]
        // public void Ordering_by_subject_scope_type()
        // {
        //     var markupSubject = new MarkupSubjectInfo
        //     {
        //         AgencyAncestors = new List<int>(),
        //         AgencyId = 1, 
        //         AgentId = 1, 
        //         CountryHtId = "Russia",
        //         LocalityHtId = "Moscow"
        //     };

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
        //             Id = 9,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Country,
        //             DestinationScopeId = "UAE"
        //         },
        //         new()
        //         {
        //             Id = 7,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Global
        //         },
        //         new()
        //         {
        //             Id = 5,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Accommodation,
        //             DestinationScopeId = "President Hotel"
        //         },
        //         new()
        //         {
        //             Id = 2,
        //             SubjectScopeType = SubjectMarkupScopeTypes.Global,
        //             DestinationScopeType = DestinationMarkupScopeTypes.Locality,
        //             DestinationScopeId = "Dubai"
        //         }
        //     };

        //     var service = MarkupPolicyServiceMock.Create(markupPolicies);

        //     var policies = service.Get(markupSubject, markupDestination);

        //     Assert.Equal(DestinationMarkupScopeTypes.Global, policies[0].DestinationScopeType);
        //     Assert.Equal(DestinationMarkupScopeTypes.Country, policies[1].DestinationScopeType);
        //     Assert.Equal(DestinationMarkupScopeTypes.Locality, policies[2].DestinationScopeType);
        //     Assert.Equal(DestinationMarkupScopeTypes.Accommodation, policies[3].DestinationScopeType);
        // }
    }
}