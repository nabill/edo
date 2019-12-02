using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Services.Markups.Availability;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        public AvailabilityResultsCache(IMemoryFlow flow)
        {
            _flow = flow;
        }


        public Task Set(AvailabilityDetailsWithMarkup availabilityResponse)
        {
            _flow.Set(
                _flow.BuildKey(KeyPrefix, availabilityResponse.ResultResponse.AvailabilityId.ToString()),
                availabilityResponse,
                ExpirationPeriod);

            return Task.CompletedTask;
        }


        public Task<AvailabilityDetailsWithMarkup> Get(int id)
        {
            _flow.TryGetValue<AvailabilityDetailsWithMarkup>(_flow.BuildKey(KeyPrefix, id.ToString()),
                out var availabilityResponse);
            return Task.FromResult(availabilityResponse);
        }
        

        private const string KeyPrefix = nameof(AvailabilityDetails) + "AvailabilityResults";
        private static readonly TimeSpan ExpirationPeriod = TimeSpan.FromHours(1);
        private readonly IMemoryFlow _flow;
    }
}