using System;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Markups;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Markups.Mocks
{
    public static class MarkupPolicyStorageMock
    {
        public static MarkupPolicyStorage Create()
        {
            var monitor = Mock.Of<IOptionsMonitor<MarkupPolicyStorageOptions>>(_ => _.CurrentValue == new MarkupPolicyStorageOptions
            {
                Timeout = TimeSpan.FromMilliseconds(1)
            });
            
            return new MarkupPolicyStorage(monitor);
        }
    }
}