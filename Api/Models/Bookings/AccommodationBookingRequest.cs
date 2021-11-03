using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string itineraryNumber, string nationality, string residency, 
            string clientReferenceCode, List<BookingRoomDetails> roomDetails, List<AccommodationFeature> features,
            Guid searchId, string htId, Guid roomContractSetId, string mainPassengerName,
            string evaluationToken, bool rejectIfUnavailable = true)
        {
            ItineraryNumber = itineraryNumber;
            Nationality = nationality;
            RejectIfUnavailable = rejectIfUnavailable;
            Residency = residency;
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
        ///     The nationality of a main passenger.
        /// </summary>
        [Required]
        public string Nationality { get; }

        /// <summary>
        ///     This indicates the system to reject the request when an accommodation has been booked by some one else between
        ///     availability and booking requests. Default is true.
        /// </summary>
        public bool RejectIfUnavailable { get; }


        /// <summary>
        ///     The residency of a main passenger.
        /// </summary>
        [Required]
        public string Residency { get; }

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
        [Required]
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
        public string ClientReferenceCode { get; }
    }
}