using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SingleAccommodationAvailability
    {
        public SingleAccommodationAvailability(
            string availabilityId,
            DateTime checkInDate,
            List<RoomContractSet> roomContractSets,
            string htId)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            HtId = htId;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>(0);
        }

        public string AvailabilityId { get; }

        public DateTime CheckInDate { get; }

        public string HtId { get; }

        public List<RoomContractSet> RoomContractSets { get; }
    }
}