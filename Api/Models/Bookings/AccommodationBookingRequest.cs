using HappyTravel.Edo.Api.Models.Accommodations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string itineraryNumber, string clientReferenceCode,
            List<BookingRoomDetails> roomDetails, List<AccommodationFeature> features,
            Guid searchId, string htId, Guid roomContractSetId, string mainPassengerName,
            string evaluationToken, bool rejectIfUnavailable = true)
        {
            ItineraryNumber = itineraryNumber;
            RejectIfUnavailable = rejectIfUnavailable;
            SearchId = searchId;
            HtId = htId;
            RoomContractSetId = roomContractSetId;
            EvaluationToken = evaluationToken;
            MainPassengerName = mainPassengerName.Trim();
            ClientReferenceCode = clientReferenceCode;

            RoomDetails = roomDetails ?? new List<BookingRoomDetails>(0);
            Features = features ?? new List<AccommodationFeature>(0);
        }

        /// <summary>
        ///     This indicates the system to reject the request when an accommodation has been booked by some one else between
        ///     availability and booking requests. Default is true.
        /// </summary>
        public bool RejectIfUnavailable { get; }

        public Guid SearchId { get; }
        public string HtId { get; }

        /// <summary>
        ///     Room details from an availability response.
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }

        /// <summary>
        ///     The selected additional accommodation features, if any.
        /// </summary>
        public List<AccommodationFeature> Features { get; }

        /// <summary>
        ///     Identifier of chosen room contract set.
        /// </summary>
        [Required]
        public Guid RoomContractSetId { get; }

        /// <summary>
        /// Token from third step availability search to disallow double bookings
        /// </summary>
        public string EvaluationToken { get; }

        /// <summary>
        ///     The full name of main passenger (buyer).
        /// </summary>
        [Required]
        public string MainPassengerName { get; }

        /// <summary>
        ///     Itinerary number to combine several orders in one pack.
        /// </summary>
        public string ItineraryNumber { get; }
        
        
        /// <summary>
        ///     Client booking reference code
        /// </summary>
        public string? ClientReferenceCode { get; }
    }
}