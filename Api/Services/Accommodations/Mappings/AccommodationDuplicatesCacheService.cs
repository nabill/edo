using System;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Mappings
{
    public class AccommodationDuplicatesCacheService
    {
        public AccommodationDuplicatesCacheService(IMemoryFlow flow)
        {
            _flow = flow;
        }


        public void SetDuplicate(DataProviders dataProvider, string accommodationId)
        {
            var key = BuildCacheKey(dataProvider, accommodationId);
            _flow.Set(key, true, TimeSpan.MaxValue);
        }
        

        public bool HasDuplicate(DataProviders dataProvider, string accommodationId)
        {
            var key = BuildCacheKey(dataProvider, accommodationId);
            return _flow.TryGetValue(key, out bool hasDuplicate)
                ? hasDuplicate
                : false;
        }
        
        
        private string BuildCacheKey(DataProviders dataProvider, string accommodationId) => _flow.BuildKey(nameof(AccommodationDuplicatesReportService),
            "Duplicates", dataProvider.ToString(),
            accommodationId);
        
        
        private readonly IMemoryFlow _flow;
    }
}