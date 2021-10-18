using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string itineraryNumber, string nationality, string residency, 
            List<BookingRoomDetails> roomDetails, List<AccommodationFeature> features,
            Guid searchId, string htId, Guid roomContractSetId, string mainPassengerName,
            bool rejectIfUnavailable = true)
        {
            ItineraryNumber = itineraryNumber;
            Nationality = nationality;
            RejectIfUnavailable = rejectIfUnavailable;
            Residency = residency;
            SearchId = searchId;
            HtId = htId;
            RoomContractSetId = roomContractSetId;
            MainPassengerName = mainPassengerName.Trim();
            RoomDetails = roomDetails;
            Features = features;
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
        ///     The full name of main passenger (buyer).
        /// </summary>
        [Required]
        public string MainPassengerName { get; }

        /// <summary>
        ///     Itinerary number to combine several orders in one pack.
        /// </summary>
        public string ItineraryNumber { get; }
    }
}