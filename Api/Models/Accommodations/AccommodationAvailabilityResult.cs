using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationAvailabilityResult
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
        
        public long Timestamp { get; }
        public string AvailabilityId { get; }
        public List<RoomContractSet> RoomContractSets { get; }
        public decimal MinPrice { get; }
        public decimal MaxPrice { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public string HtId { get; }
        public string SupplierAccommodationCode { get; }


        public bool Equals(AccommodationAvailabilityResult other)
        {
            return Timestamp == other.Timestamp && AvailabilityId == other.AvailabilityId &&
                Equals(RoomContractSets, other.RoomContractSets) &&
                MinPrice == other.MinPrice && MaxPrice == other.MaxPrice &&
                HtId == other.HtId;
        }


        public override bool Equals(object obj) 
            => obj is AccommodationAvailabilityResult other && Equals(other);

        
        public override int GetHashCode() 
            => HashCode.Combine(Timestamp, AvailabilityId, RoomContractSets, MinPrice, MaxPrice);

    }
}