using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AccommodationAvailabilityResult
    {
        [JsonConstructor]
        public AccommodationAvailabilityResult(Guid searchId, string supplierCode, DateTimeOffset created, string availabilityId,
            List<RoomContractSet> roomContractSets, decimal minPrice, decimal maxPrice, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, string htId, string supplierAccommodationCode, string countryHtId,
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
            HtId = htId;
            SupplierAccommodationCode = supplierAccommodationCode;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            MarketId = marketId;
            CountryCode = countryCode;
        }

        [JsonIgnore]
        public ObjectId Id { get; init; }
        public Guid SearchId { get; init; }
        public string SupplierCode { get; init; }
        public DateTimeOffset Created { get; init; }
        public string AvailabilityId { get; init; }
        public List<RoomContractSet> RoomContractSets { get; init; }
        public decimal MinPrice { get; init; }
        public decimal MaxPrice { get; init; }
        public DateTimeOffset CheckInDate { get; init; }
        public DateTimeOffset CheckOutDate { get; init; }
        public string HtId { get; init; }
        public string SupplierAccommodationCode { get; init; }
        public string CountryHtId { get; init; }
        public string LocalityHtId { get; init; }
        public int MarketId { get; init; }
        public string CountryCode { get; init; }
    }
}