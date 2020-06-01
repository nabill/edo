using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using Microsoft.Extensions.Options;
using Moq;

namespace HappyTravel.Edo.UnitTests.Availability.Step1.Storage
{
    public static class AvailabilityStorageUtils
    {
        public static IAvailabilityStorage CreateEmptyStorage(IOptions<DataProviderOptions> providerOptions)
        {
            return new AvailabilityStorage(new InMemoryDistributedFlow(),
                Mock.Of<IMemoryFlow>(), 
                providerOptions);
        }
    }
}