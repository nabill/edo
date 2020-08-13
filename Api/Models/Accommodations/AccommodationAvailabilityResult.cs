using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationAvailabilityResult
    {
        public AccommodationAvailabilityResult(Guid id,
            long timestamp,
            string availabilityId,
            SlimAccommodationDetails accommodationDetails,
            List<RoomContractSet> roomContractSets,
            decimal minPrice,
            decimal maxPrice)
        {
            Id = id;
            Timestamp = timestamp;
            AvailabilityId = availabilityId;
            AccommodationDetails = accommodationDetails;
            RoomContractSets = roomContractSets;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }
        
        public Guid Id { get; }
        public long Timestamp { get; }
        public string AvailabilityId { get; }
        public SlimAccommodationDetails AccommodationDetails { get; }
        public List<RoomContractSet> RoomContractSets { get; }
        public decimal MinPrice { get; }
        public decimal MaxPrice { get; }
    }
}