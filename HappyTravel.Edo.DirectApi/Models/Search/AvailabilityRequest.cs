using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct AvailabilityRequest
    {
        [JsonConstructor]
        public AvailabilityRequest(List<string> ids, string nationality, string residency, DateTime checkInDate, DateTime checkOutDate, List<RoomOccupationRequest> roomDetails)
        {
            Ids = ids;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Nationality = nationality;
            Residency = residency;
            RoomDetails = roomDetails;
        }
        
        
        /// <summary>
        ///     IDs for countries, localities, or accommodations
        /// </summary>
        public List<string> Ids { get; }

        /// <summary>
        ///     Check-in date
        /// </summary>
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     Check-out date
        /// </summary>
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     Uppercase two-letter country code for the lead passenger's nationality (see Alpha-2 codes at https://www.iban.com/country-codes)
        /// </summary>
        public string Nationality { get; }

        /// <summary>
        ///     Uppercase two-letter country code for the lead passenger's residency (see Alpha-2 codes at https://www.iban.com/country-codes)
        /// </summary>
        public string Residency { get; }

        /// <summary>
        ///     Desired room details
        /// </summary>
        public List<RoomOccupationRequest> RoomDetails { get; }
    }
}