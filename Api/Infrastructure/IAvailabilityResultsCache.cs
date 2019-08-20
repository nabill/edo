using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Availabilities;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IAvailabilityResultsCache
    {
        Task Save(IEnumerable<SlimAvailabilityResult> availabilities);
        Task<SlimAvailabilityResult> Get(Guid id);
    }
}