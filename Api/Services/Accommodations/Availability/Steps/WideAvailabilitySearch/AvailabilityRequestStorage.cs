using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class AvailabilityRequestStorage : IAvailabilityRequestStorage
    {
        public AvailabilityRequestStorage(IDoubleFlow flow, IOptionsMonitor<AvailabilityRequestStorageOptions> options)
        {
            _flow = flow;
            _lifeTime = options.CurrentValue.StorageLifeTime;
        }


        public Task Set(Guid searchId, AvailabilityRequest request) 
            => _flow.SetAsync(BuildKey(searchId), request, _lifeTime);


        public async Task<Result<AvailabilityRequest>> Get(Guid searchId)
        {
            var key = BuildKey(searchId);
            var request = await _flow.GetAsync<AvailabilityRequest?>(key, _lifeTime);
            return request ?? Result.Failure<AvailabilityRequest>("Could not find search request in cache");
        }


        private string BuildKey(Guid searchId) 
            => _flow.BuildKey(nameof(AvailabilityRequestStorage), searchId.ToString());


        private readonly TimeSpan _lifeTime;
        private readonly IDoubleFlow _flow;
    }
}