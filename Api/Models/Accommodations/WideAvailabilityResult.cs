using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(SlimAccommodation accommodation, List<RoomContractSet> roomContractSets, decimal minPrice,
            decimal maxPrice, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, DateTimeOffset expiredAfter, string? supplierCode, string htId)
        {
            Accommodation = accommodation;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            ExpiredAfter = expiredAfter;
            SupplierCode = supplierCode;
            HtId = htId;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>();
        }


        public WideAvailabilityResult(WideAvailabilityResult result, List<RoomContractSet> roomContractSets)
            : this(result.Accommodation, roomContractSets, result.MinPrice, result.MaxPrice, result.CheckInDate,
                result.CheckOutDate, result.ExpiredAfter, result.SupplierCode, result.HtId)
        { }
        
        /// <summary>
        /// Accommodation data
        /// </summary>
        public SlimAccommodation Accommodation { get; }

        /// <summary>
        /// Minimal room contract set price
        /// </summary>
        public decimal MinPrice { get; }
        
        /// <summary>
        /// Maximal room contract set price
        /// </summary>
        public decimal MaxPrice { get; }

        /// <summary>
        /// Check in date
        /// </summary>
        public DateTimeOffset CheckInDate { get; }
        
        /// <summary>
        /// Check out date
        /// </summary>
        public DateTimeOffset CheckOutDate { get; }
        
        /// <summary>
        /// Expiration date
        /// </summary>>
        public DateTimeOffset ExpiredAfter { get; }

        /// <summary>
        /// Supplier code
        /// </summary>
        public string? SupplierCode { get; }


        /// <summary>
        /// HT id
        /// </summary>
        public string HtId { get; }


        /// <summary>
        /// List of available room contracts sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}