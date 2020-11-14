using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
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
            Suppliers? supplier)
        {
            Id = id;
            Accommodation = accommodation;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            HasDuplicate = hasDuplicate;
            Supplier = supplier;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>();
        }


        public WideAvailabilityResult(WideAvailabilityResult result, List<RoomContractSet> roomContractSets)
            : this(result.Id, result.Accommodation, roomContractSets, result.MinPrice, result.MaxPrice, result.HasDuplicate, result.Supplier)
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
        /// Temporarily added data provider for filtering and testing purposes. 
        /// </summary>
        public Suppliers? Supplier { get; }

        
        /// <summary>
        /// List of available room contracts sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}