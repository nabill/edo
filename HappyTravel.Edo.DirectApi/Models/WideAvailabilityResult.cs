using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(string accommodationId, List<RoomContractSet> roomContractSets, decimal minPrice,
            decimal maxPrice, DateTime checkInDate, DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            RoomContractSets = roomContractSets;
        }
        
        /// <summary>
        /// AccommodationId
        /// </summary>
        public string AccommodationId { get; }
        
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
        /// List of available room contracts sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}