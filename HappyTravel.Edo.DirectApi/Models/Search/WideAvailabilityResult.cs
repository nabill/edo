using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct WideAvailabilityResult
    {
        [JsonConstructor]
        public WideAvailabilityResult(string accommodationId, List<RoomContractSet> roomContractSets, DateTimeOffset checkInDate, DateTimeOffset checkOutDate, DateTimeOffset expiredAfter)
        {
            AccommodationId = accommodationId;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            ExpiredAfter = expiredAfter;
            RoomContractSets = roomContractSets;
        }
        
        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }

        /// <summary>
        ///     Check-in date
        /// </summary>
        public DateTimeOffset CheckInDate { get; }
        
        /// <summary>
        ///     Check-out date
        /// </summary>
        public DateTimeOffset CheckOutDate { get; }
        
        /// <summary>
        ///     Expiration date
        /// </summary>>
        public DateTimeOffset ExpiredAfter { get; }

        /// <summary>
        ///     List of available room contracts sets
        /// </summary>
        public List<RoomContractSet> RoomContractSets { get; }
    }
}