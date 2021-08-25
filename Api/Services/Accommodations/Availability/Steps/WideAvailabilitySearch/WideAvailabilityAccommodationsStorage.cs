using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public class WideAvailabilityAccommodationsStorage : IWideAvailabilityAccommodationsStorage
    {
        public WideAvailabilityAccommodationsStorage(IMemoryFlow flow, IAccommodationMapperClient mapperClient)
        {
            _flow = flow;
            _mapperClient = mapperClient;
        }
        
        
        public async ValueTask EnsureAccommodationsCached(List<string> htIds, string languageCode)
        {
            var nonCachedIds = new List<string>();
            foreach (var htId in htIds)
            {
                if (!_flow.TryGetValue<SlimAccommodation>(BuildKey(htId, languageCode), out _))
                    nonCachedIds.Add(htId);
            }
            if (!nonCachedIds.Any())
                return;

            var accommodationsToCache = await _mapperClient.GetAccommodations(nonCachedIds, languageCode);
            foreach (var accommodation in accommodationsToCache)
                _flow.Set(BuildKey(accommodation.HtId, languageCode), accommodation, AccommodationCacheLifeTime);
        }


        public SlimAccommodation GetAccommodation(string htId, string languageCode)
        {
            _flow.TryGetValue(BuildKey(htId, languageCode), out MapperContracts.Public.Accommodations.SlimAccommodation accommodation);
            return accommodation.ToEdoContract();
        }


        private string BuildKey(string htId, string languageCode) 
            => _flow.BuildKey(nameof(WideAvailabilityAccommodationsStorage), languageCode, htId);

        
        private readonly IMemoryFlow _flow;
        private readonly IAccommodationMapperClient _mapperClient;

        private static readonly TimeSpan AccommodationCacheLifeTime = TimeSpan.FromHours(1);
    }
}