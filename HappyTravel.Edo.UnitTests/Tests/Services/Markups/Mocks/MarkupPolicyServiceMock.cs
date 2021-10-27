using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Data.Markup;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks
{
    public static class MarkupPolicyServiceMock
    {
        public static MarkupPolicyService Create(List<MarkupPolicy> markupPolicies)
        {
            var storage = MarkupPolicyStorageMock.Create();
            storage.Set(markupPolicies);
            return new MarkupPolicyService(storage);
        } 
    }
}