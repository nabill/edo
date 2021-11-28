using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups
{
    public class DiscountStorageTests
    {
        [Fact]
        private void Should_throw_exception_if_storage_is_not_filled()
        {
            var discountStorage = DiscountStorageMock.Create();
            Assert.Throws<Exception>(() => discountStorage.Get(x => x.Id == 1));
        }


        [Fact]
        private void Should_return_results_if_storage_is_filled()
        {
            var discountStorage = DiscountStorageMock.Create();
            discountStorage.Set(new List<Discount>());
            var result = discountStorage.Get(x => x.Id == 1);
            Assert.NotNull(result);
        }
    }
}