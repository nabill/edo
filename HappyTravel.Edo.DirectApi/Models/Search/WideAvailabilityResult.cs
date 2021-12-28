using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(string accommodationId, List<RoomContractSet> roomContractSets
            , DateTime checkInDate, DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            RoomContractSets = roomContractSets;
        }
        
        /// <summary>
        /// AccommodationId
        /// </summary>
        public string AccommodationId { get; }

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