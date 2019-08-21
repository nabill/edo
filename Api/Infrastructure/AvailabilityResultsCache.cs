using System.Collections.Concurrent;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        private readonly ConcurrentDictionary<int, AvailabilityResponse> _cache =
            new ConcurrentDictionary<int, AvailabilityResponse>();

        public Task Save(AvailabilityResponse availabilityResponse)
        {
            _cache.AddOrUpdate(availabilityResponse.AvailabilityId, availabilityResponse,
                (guid, result) => availabilityResponse);
            
            return Task.CompletedTask;
        }

        public Task<AvailabilityResponse> Get(int id)
        {
            _cache.TryGetValue(id, out var availabilityResult);
            return Task.FromResult(availabilityResult);
        }
    }
}