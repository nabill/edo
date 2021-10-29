using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups
{
    public class MarkupPolicyStorageTests
    {
        [Fact]
        private void Should_throw_exception_if_storage_is_not_filled()
        {
            var markupPolicyStorage = MarkupPolicyStorageMock.Create();
            Assert.Throws<Exception>(() => markupPolicyStorage.Get(x => x.Id == 1));
        }


        [Fact]
        private void Should_return_results_if_storage_is_filled()
        {
            var markupPolicyStorage = MarkupPolicyStorageMock.Create();
            markupPolicyStorage.Set(new List<MarkupPolicy>());
            var result = markupPolicyStorage.Get(x => x.Id == 1);
            Assert.NotNull(result);
        }
    }
}