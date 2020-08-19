using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationAvailabilityResult
    {
        [JsonConstructor]
        public AccommodationAvailabilityResult(Guid id,
            long timestamp,
            string availabilityId,
            SlimAccommodationDetails accommodationDetails,
            List<RoomContractSet> roomContractSets,
            string duplicateReportId,
            decimal minPrice,
            decimal maxPrice)
        {
            Id = id;
            Timestamp = timestamp;
            AvailabilityId = availabilityId;
            AccommodationDetails = accommodationDetails;
            RoomContractSets = roomContractSets;
            DuplicateReportId = duplicateReportId;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }
        
        public Guid Id { get; }
        public long Timestamp { get; }
        public string AvailabilityId { get; }
        public SlimAccommodationDetails AccommodationDetails { get; }
        public List<RoomContractSet> RoomContractSets { get; }
        public string DuplicateReportId { get; }
        public decimal MinPrice { get; }
        public decimal MaxPrice { get; }
    }
}