using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection
{
    public static class RoomSelectionPolicyProcessor
    {
        public static SingleAccommodationAvailability Process(SingleAccommodationAvailability accommodationAvailability, CancellationPolicyProcessSettings settings)
        {
            var processedRoomContractSets = RoomContractSetPolicyProcessor.Process(accommodationAvailability.RoomContractSets,
                accommodationAvailability.CheckInDate,
                settings);

            return SetRoomContractSets(accommodationAvailability, processedRoomContractSets);


            static SingleAccommodationAvailability SetRoomContractSets(in SingleAccommodationAvailability availability, List<RoomContractSet> roomContractSets)
                => new (availabilityId: availability.AvailabilityId,
                    checkInDate: availability.CheckInDate,
                    roomContractSets: roomContractSets,
                    htId: availability.HtId,
                    countryHtId: availability.CountryHtId,
                    localityHtId: availability.LocalityHtId);
        }
    }
}