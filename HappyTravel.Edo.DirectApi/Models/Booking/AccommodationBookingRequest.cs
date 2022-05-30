using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string accommodationId, string clientReferenceCode, string evaluationToken,
            List<BookingRoomDetails> roomDetails, Guid searchId, Guid roomContractSetId)
        {
            AccommodationId = accommodationId;
            SearchId = searchId;
            RoomContractSetId = roomContractSetId;
            ClientReferenceCode = clientReferenceCode;
            EvaluationToken = evaluationToken;
            RoomDetails = roomDetails;
        }
        
        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string AccommodationId { get; }
        
        /// <summary>
        ///     ID for the search
        /// </summary>
        public Guid SearchId { get; }
        
        /// <summary>
        ///     ID for the room contract set you want to book
        /// </summary>
        public Guid RoomContractSetId { get; }
        
        /// <summary>
        ///     Client booking reference code
        /// </summary>
        public string ClientReferenceCode { get; }
        
        /// <summary>
        ///     Evaluation token
        /// </summary>
        public  string EvaluationToken { get; }
        
        /// <summary>
        ///     Room details that match the response from the booking evaluation step
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }
    }
}