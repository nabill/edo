using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.SuppliersCatalog;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(Guid id, 
            SlimAccommodation accommodation,
            List<RoomContractSet> roomContractSets,
            decimal minPrice,
            decimal maxPrice,
            bool hasDuplicate,
            DateTime checkInDate,
            DateTime checkOutDate,
            Suppliers? supplier,
            string htId)
        {
            Id = id;
            Accommodation = accommodation;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            HasDuplicate = hasDuplicate;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Supplier = supplier;
            HtId = htId;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>();
        }


        public WideAvailabilityResult(WideAvailabilityResult result, List<RoomContractSet> roomContractSets)
            : this(result.Id, result.Accommodation, roomContractSets, result.MinPrice, result.MaxPrice, result.HasDuplicate,
                result.CheckInDate, result.CheckOutDate, result.Supplier, result.HtId)
        { }
        
        public Guid Id { get; }

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
        /// Indicates that accommodation of this availability has duplicates in other connectors.
        /// </summary>
        public bool HasDuplicate { get; }

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