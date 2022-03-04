using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SingleAccommodationAvailability
    {
        public SingleAccommodationAvailability(
            string availabilityId,
            DateTimeOffset checkInDate,
            List<RoomContractSet> roomContractSets,
            string htId,
            string countryHtId,
            string localityHtId)
        {
            AvailabilityId = availabilityId;
            CheckInDate = checkInDate;
            HtId = htId;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>(0);
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
        }

        public string AvailabilityId { get; }

        public DateTimeOffset CheckInDate { get; }

        public string HtId { get; }

        public List<RoomContractSet> RoomContractSets { get; }
        
        public string CountryHtId { get; }
        
        public string LocalityHtId { get; }
    }
}