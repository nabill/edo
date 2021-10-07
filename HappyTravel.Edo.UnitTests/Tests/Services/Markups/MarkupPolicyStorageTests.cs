using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Data.Markup;
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
            var markupPolicyStorage = GetStorage();
            Assert.Throws<Exception>(() => markupPolicyStorage.Get(x => x.Id == 1));
        }


        [Fact]
        private void Should_return_results_if_storage_is_filled()
        {
            var markupPolicyStorage = GetStorage();
            markupPolicyStorage.Set(new List<MarkupPolicy>());
            var result = markupPolicyStorage.Get(x => x.Id == 1);
            Assert.NotNull(result);
        }


        private MarkupPolicyStorage GetStorage()
        {
            var monitor = Mock.Of<IOptionsMonitor<MarkupPolicyStorageOptions>>(_ => _.CurrentValue == new MarkupPolicyStorageOptions
            {
                Timeout = TimeSpan.FromMilliseconds(1)
            });
            
            return new MarkupPolicyStorage(monitor);
        }
    }
}