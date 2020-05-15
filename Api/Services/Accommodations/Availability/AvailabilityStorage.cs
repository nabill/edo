using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using Microsoft.Extensions.Caching.Distributed;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityStorage 
    {
        public AvailabilityStorage(IJsonSerializer serializer,
            IDistributedCache distributedCache,
            IMemoryFlow memoryFlow)
        {
            _serializer = serializer;
            _distributedCache = distributedCache;
            _memoryFlow = memoryFlow;
        }


        public Task SaveResult(Guid searchId, CombinedAvailabilityDetails details) => SaveObject(searchId, details);
        
        public Task SaveState(Guid searchId, AvailabilitySearchState searchState) => SaveObject(searchId, searchState);
        
        public ValueTask<CombinedAvailabilityDetails> GetResult(Guid searchId)
        {
            var key = BuildKey<CombinedAvailabilityDetails>(searchId);
            return _memoryFlow.GetOrSetAsync(key,
                async () =>
                {
                    var resultString = await _distributedCache.GetStringAsync(key);
                    return _serializer.DeserializeObject<CombinedAvailabilityDetails>(resultString);
                }, CacheExpirationTime);
        }


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var key = BuildKey<AvailabilitySearchState>(searchId);
            var resultString = await _distributedCache.GetStringAsync(key);
            if(string.IsNullOrWhiteSpace(resultString))
                return new AvailabilitySearchState(AvailabilitySearchTaskState.NotFound);
            
            return _serializer.DeserializeObject<AvailabilitySearchState>(resultString);
        }
        
        
        private Task SaveObject<TObjectType>(Guid searchId, TObjectType @object)
        {
            var key = BuildKey<TObjectType>(searchId);
            var resultString = _serializer.SerializeObject(@object);
            return _distributedCache.SetStringAsync(key, resultString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpirationTime
            });
        }
        
        private string BuildKey<TObjectType>(Guid searchId) => $"{nameof(AvailabilityStorage)}::AVAILABILITY::{searchId}::{typeof(TObjectType).Name}";

        private readonly IJsonSerializer _serializer;
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryFlow _memoryFlow;
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
    }
}