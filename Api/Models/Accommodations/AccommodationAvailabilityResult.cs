using System;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AccommodationAvailabilityResult
    {
        public AccommodationAvailabilityResult(Guid searchId, string supplierCode, DateTimeOffset created, string availabilityId,
            List<RoomContractSet> roomContractSets, decimal minPrice, decimal maxPrice, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, 
            DateTimeOffset expiredAfter, string htId, string supplierAccommodationCode, string countryHtId,
            string localityHtId, int marketId, string countryCode)
        {
            SearchId = searchId;
            SupplierCode = supplierCode;
            Created = created;
            AvailabilityId = availabilityId;
            RoomContractSets = roomContractSets;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            ExpiredAfter = expiredAfter;
            HtId = htId;
            SupplierAccommodationCode = supplierAccommodationCode;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            MarketId = marketId;
            CountryCode = countryCode;
        }
        
        public Guid SearchId { get; }
        public string SupplierCode { get; }
        public DateTimeOffset Created { get; }
        public string AvailabilityId { get; }
        public List<RoomContractSet> RoomContractSets { get; init; }
        public decimal MinPrice { get; }
        public decimal MaxPrice { get; }
        public DateTimeOffset CheckInDate { get; }
        public DateTimeOffset CheckOutDate { get; }
        public DateTimeOffset ExpiredAfter { get; }
        public string HtId { get; }
        public string SupplierAccommodationCode { get; }
        public string CountryHtId { get; }
        public string LocalityHtId { get; }
        public int MarketId { get; }
        public string CountryCode { get; }
    }
}