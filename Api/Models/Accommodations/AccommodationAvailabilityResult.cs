using System;
using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public record AccommodationAvailabilityResult
    {
        [JsonConstructor]
        public AccommodationAvailabilityResult(Guid searchId, Suppliers supplier, DateTime created, string availabilityId, List<RoomContractSet> roomContractSets,
            decimal minPrice, decimal maxPrice, DateTime checkInDate, DateTime checkOutDate, string htId, string supplierAccommodationCode)
        {
            SearchId = searchId;
            Supplier = supplier;
            Created = created;
            AvailabilityId = availabilityId;
            RoomContractSets = roomContractSets;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            HtId = htId;
            SupplierAccommodationCode = supplierAccommodationCode;
        }
        
        [JsonIgnore]
        public ObjectId Id { get; init; }
        public Guid SearchId { get; init; }
        public Suppliers Supplier { get; init; }
        public DateTime Created { get; init; }
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