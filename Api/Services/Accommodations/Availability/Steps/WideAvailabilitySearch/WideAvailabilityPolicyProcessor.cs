using System.Linq;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class WideAvailabilityPolicyProcessor
    {
        public static EdoContracts.Accommodations.Availability Process(EdoContracts.Accommodations.Availability availability, CancellationPolicyProcessSettings settings)
        {
            var results = availability.Results
                .Select(r =>
                {
                    return new SlimAccommodationAvailability(
                        r.Accommodation, 
                        RoomContractSetPolicyProcessor.Process(r.RoomContractSets, availability.CheckInDate, settings),
                        r.AvailabilityId);
                })
                .ToList();

            return new EdoContracts.Accommodations.Availability(availability.AvailabilityId,
                availability.NumberOfNights, availability.CheckInDate,
                availability.CheckOutDate,
                results,
                availability.NumberOfProcessedAccommodations);
        }
    }
}