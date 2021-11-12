using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.DirectApi.Models
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
        ///     Alpha-2 nationality code for a lead passengers.
        /// </summary>
        public string Nationality { get; }

        /// <summary>
        ///     Alpha-2 residency code for a lead passengers.
        /// </summary>
        public string Residency { get; }

        /// <summary>
        ///     Desirable room details.
        /// </summary>
        public List<RoomOccupationRequest> RoomDetails { get; }
    }
}