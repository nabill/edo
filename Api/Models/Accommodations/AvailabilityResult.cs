using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AvailabilityResult
    {
        [JsonConstructor]
        public AvailabilityResult(Guid resultId, 
            SlimAccommodationDetails accommodationDetails,
            List<RoomContractSet> roomContractSets,
            decimal minPrice,
            decimal maxPrice,
            bool hasDuplicate)
        {
            ResultId = resultId;
            AccommodationDetails = accommodationDetails;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            HasDuplicate = hasDuplicate;
            RoomContractSets = roomContractSets ?? new List<RoomContractSet>();
        }


        public AvailabilityResult(AvailabilityResult result, List<RoomContractSet> roomContractSets)
            : this(result.ResultId, result.AccommodationDetails, roomContractSets, result.MinPrice, result.MaxPrice, result.HasDuplicate)
        { }
        
        public Guid ResultId { get; }

        /// <summary>
        /// Accommodation data
        /// </summary>
        public SlimAccommodationDetails AccommodationDetails { get; }

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
        /// List of available room contracts sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}