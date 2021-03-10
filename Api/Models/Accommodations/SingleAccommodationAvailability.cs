using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public class SingleAccommodationAvailability
    {
        public SingleAccommodationAvailability(
            string availabilityId,
            DateTime checkInDate,
            List<EdoContracts.Accommodations.Internals.RoomContractSet> roomContractSets,
            string htId)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            HtId = htId;
            RoomContractSets = roomContractSets ?? new List<EdoContracts.Accommodations.Internals.RoomContractSet>(0);
        }

        public string AvailabilityId { get; }

        public DateTime CheckInDate { get; }

        public string HtId { get; }

        public List<EdoContracts.Accommodations.Internals.RoomContractSet> RoomContractSets { get; }
    }
}