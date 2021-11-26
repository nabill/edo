using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string accommodationId, string nationality, string residency, 
            string referenceCode, List<BookingRoomDetails> roomDetails, Guid searchId, Guid roomContractSetId)
        {
            AccommodationId = accommodationId;
            Nationality = nationality;
            Residency = residency;
            SearchId = searchId;
            RoomContractSetId = roomContractSetId;
            ReferenceCode = referenceCode;
            RoomDetails = roomDetails;
        }
        
        /// <summary>
        ///     Accommodation Id
        /// </summary>
        public string AccommodationId { get; }
        
        /// <summary>
        ///     Search Id
        /// </summary>
        public Guid SearchId { get; }
        
        /// <summary>
        ///     Identifier of chosen room contract set.
        /// </summary>
        public Guid RoomContractSetId { get; }
        
        /// <summary>
        ///     Client booking reference code
        /// </summary>
        public string ReferenceCode { get; }
        
        /// <summary>
        ///     Alpha-2 nationality code for a lead passengers.
        /// </summary>
        public string Nationality { get; }

        /// <summary>
        ///     Alpha-2 residency of a main passenger.
        /// </summary>
        public string Residency { get; }
        
        /// <summary>
        ///     Room details from an availability response.
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }
    }
}