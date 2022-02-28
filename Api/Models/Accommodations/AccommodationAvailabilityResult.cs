using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AccommodationAvailabilityResult
    {
        [JsonConstructor]
        public AccommodationAvailabilityResult(Guid searchId, string supplierCode, DateTime created, string availabilityId,
            List<RoomContractSet> roomContractSets, decimal minPrice, decimal maxPrice, DateTime checkInDate, DateTime checkOutDate, string htId, string supplierAccommodationCode, string countryHtId,
            string localityHtId)
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
        }
        
        [JsonIgnore]
        public ObjectId Id { get; init; }
        public Guid SearchId { get; init; }
        public string SupplierCode { get; init; }
        public DateTime Created { get; init; }
        public string AvailabilityId { get; init; }
        public List<RoomContractSet> RoomContractSets { get; init; }
        public decimal MinPrice { get; init; }
        public decimal MaxPrice { get; init; }
        public DateTime CheckInDate { get; init; }
        public DateTime CheckOutDate { get; init; }
        public string HtId { get; init; }
        public string SupplierAccommodationCode { get; init; }
        public string CountryHtId { get; init; }
        public string LocalityHtId { get; init; }

    }
}