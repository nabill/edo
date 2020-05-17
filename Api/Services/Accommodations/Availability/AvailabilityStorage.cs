using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class AvailabilityStorage 
    {
        public AvailabilityStorage(IJsonSerializer serializer,
            IDoubleFlow doubleFlow)
        {
            _doubleFlow = doubleFlow;
        }


        public Task SaveResult(Guid searchId, CombinedAvailabilityDetails details) => SaveObject(searchId, details);
        
        public Task SaveState(Guid searchId, AvailabilitySearchState searchState) => SaveObject(searchId, searchState);
        
        public ValueTask<CombinedAvailabilityDetails> GetResult(Guid searchId)
        {
            var key = BuildKey<CombinedAvailabilityDetails>(searchId);
            return _doubleFlow.GetAsync<CombinedAvailabilityDetails>(key, CacheExpirationTime);
        }


        public async Task<AvailabilitySearchState> GetState(Guid searchId)
        {
            var key = BuildKey<AvailabilitySearchState>(searchId);
            var result = await _doubleFlow.GetAsync<AvailabilitySearchState>(key, CacheExpirationTime);
            return result.Equals(default)
                ? new AvailabilitySearchState(searchId, AvailabilitySearchTaskState.NotFound)
                : result;
        }
        
        
        private Task SaveObject<TObjectType>(Guid searchId, TObjectType @object)
        {
            var key = BuildKey<TObjectType>(searchId);
            return _doubleFlow.SetAsync(key, @object, CacheExpirationTime);
        }
        
        private string BuildKey<TObjectType>(Guid searchId) => _doubleFlow.BuildKey(nameof(AvailabilityStorage), searchId.ToString(), typeof(TObjectType).Name);


        private readonly IDoubleFlow _doubleFlow;
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
    }
}