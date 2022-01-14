using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string accommodationId, string clientReferenceCode, 
            List<BookingRoomDetails> roomDetails, Guid searchId, Guid roomContractSetId)
        {
            AccommodationId = accommodationId;
            SearchId = searchId;
            RoomContractSetId = roomContractSetId;
            ClientReferenceCode = clientReferenceCode;
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
        public string ClientReferenceCode { get; }
        
        /// <summary>
        ///     Room details from an availability response.
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }
    }
}