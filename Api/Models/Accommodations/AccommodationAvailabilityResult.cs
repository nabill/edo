using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AccommodationAvailabilityResult
    {
        [JsonConstructor]
        public AccommodationAvailabilityResult(long timestamp, string availabilityId, List<RoomContractSet> roomContractSets,
            decimal minPrice, decimal maxPrice, DateTime checkInDate, DateTime checkOutDate, string htId, string supplierAccommodationCode)
        {
            Timestamp = timestamp;
            AvailabilityId = availabilityId;
            RoomContractSets = roomContractSets;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            HtId = htId;
            SupplierAccommodationCode = supplierAccommodationCode;
        }
        
        public long Timestamp { get; init; }
        public string AvailabilityId { get; init; }
        public List<RoomContractSet> RoomContractSets { get; init; }
        public decimal MinPrice { get; init; }
        public decimal MaxPrice { get; init; }
        public DateTime CheckInDate { get; init; }
        public DateTime CheckOutDate { get; init; }
        public string HtId { get; init; }
        public string SupplierAccommodationCode { get; init; }

    }
}