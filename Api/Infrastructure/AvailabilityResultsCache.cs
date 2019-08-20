using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class AvailabilityResultsCache : IAvailabilityResultsCache
    {
        private readonly ConcurrentDictionary<Guid, SlimAvailabilityResult> _cache =
            new ConcurrentDictionary<Guid, SlimAvailabilityResult>();

        public Task Save(IEnumerable<SlimAvailabilityResult> availabilities)
        {
            foreach (var availabilityResult in availabilities)
                _cache.AddOrUpdate(availabilityResult.Id, availabilityResult,
                    (guid, result) => availabilityResult);
            return Task.CompletedTask;
        }

        public Task<SlimAvailabilityResult> Get(Guid id)
        {
            _cache.TryGetValue(id, out var availabilityResult);
            return Task.FromResult(availabilityResult);
        }
    }
}