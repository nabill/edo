using System;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Markups;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks
{
    public static class DiscountStorageMock
    {
        public static DiscountStorage Create()
        {
            var monitor = Mock.Of<IOptionsMonitor<DiscountStorageOptions>>(_ => _.CurrentValue == new DiscountStorageOptions
            {
                Timeout = TimeSpan.FromMilliseconds(1)
            });
            
            return new DiscountStorage(monitor);
        }
    }
}