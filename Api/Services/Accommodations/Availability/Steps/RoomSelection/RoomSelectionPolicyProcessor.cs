using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public static class RoomSelectionPolicyProcessor
    {
        public static AccommodationAvailability Process(AccommodationAvailability accommodationAvailability, CancellationPolicyProcessSettings settings)
        {
            var processedRoomContractSets = RoomContractSetPolicyProcessor.Process(accommodationAvailability.RoomContractSets,
                accommodationAvailability.CheckInDate,
                settings);

            return SetRoomContractSets(accommodationAvailability, processedRoomContractSets);


            static AccommodationAvailability SetRoomContractSets(in AccommodationAvailability availability, List<RoomContractSet> roomContractSets) => new AccommodationAvailability(availability.AvailabilityId,
                availability.CheckInDate,
                availability.CheckOutDate,
                availability.NumberOfNights,
                availability.Accommodation,
                roomContractSets);
        }
    }
}