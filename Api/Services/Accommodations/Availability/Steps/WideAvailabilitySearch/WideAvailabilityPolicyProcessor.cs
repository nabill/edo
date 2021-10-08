using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class WideAvailabilityPolicyProcessor
    {
        public static List<AccommodationAvailabilityResult> Process(List<AccommodationAvailabilityResult> results, CancellationPolicyProcessSettings settings)
        {
            return results
                .Select(r => r with { RoomContractSets = RoomContractSetPolicyProcessor.Process(r.RoomContractSets, r.CheckInDate, settings)})
                .ToList();
        }
    }
}