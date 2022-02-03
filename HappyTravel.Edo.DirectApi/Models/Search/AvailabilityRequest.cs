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
        /// Countries, Localities or Accommodations Ids
        /// </summary>
        public List<string> Ids { get; }

        /// <summary>
        ///     Check-in date.
        /// </summary>
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     Check-out date.
        /// </summary>
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     Uppercase Alpha-2 nationality code for a lead passengers. More info on https://www.iban.com/country-codes
        /// </summary>
        public string Nationality { get; }

        /// <summary>
        ///     Uppercase Alpha-2 residency code for a lead passengers. More info on https://www.iban.com/country-codes
        /// </summary>
        public string Residency { get; }

        /// <summary>
        ///     Desirable room details.
        /// </summary>
        public List<RoomOccupationRequest> RoomDetails { get; }
    }
}