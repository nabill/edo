using System;
using System.Collections.Generic;
using HappyTravel.SuppliersCatalog;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(List<RoomContractSet> roomContractSets, decimal minPrice,
            decimal maxPrice, DateTime checkInDate, DateTime checkOutDate, Suppliers? supplier, string htId)
        {
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Supplier = supplier;
            HtId = htId;
            RoomContractSets = roomContractSets;
        }
        
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
        public DateTime CheckInDate { get; }
        
        /// <summary>
        /// Check out date
        /// </summary>
        public DateTime CheckOutDate { get; }


        /// <summary>
        /// Temporarily added data supplier for filtering and testing purposes. 
        /// </summary>
        public Suppliers? Supplier { get; }

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