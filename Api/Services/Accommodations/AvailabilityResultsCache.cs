using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        public AvailabilityResultsCache(IMemoryFlow flow)
        {
            _flow = flow;
        }

        public Task Save(AvailabilityResponse availabilityResponse)
        {
            _flow.Set(
                _flow.BuildKey(KeyPrefix, availabilityResponse.AvailabilityId.ToString()),
                availabilityResponse,
                ExpirationPeriod);

            return Task.CompletedTask;
        }

        public Task<AvailabilityResponse> Get(int id)
        {
            _flow.TryGetValue<AvailabilityResponse>(_flow.BuildKey(KeyPrefix, id.ToString()),
                out var availabilityResponse);
            return Task.FromResult(availabilityResponse);
        }
        
        private const string KeyPrefix = nameof(AvailabilityResponse) + "AvailabilityResults";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(1);
        private readonly IMemoryFlow _flow;
    }
}