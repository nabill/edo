using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingRequest
    {
        [JsonConstructor]
        public AccommodationBookingRequest(string accommodationId, int availabilityId, DateTime checkInDate, DateTime checkOutDate,
            long? itineraryNumber, string nationality, PaymentMethods paymentMethod, string residency, string tariffCode,
            List<BookingRoomDetails> roomDetails, List<AccommodationFeature> features, string agentReference,
            Guid agreementId,
            bool rejectIfUnavailable = true)
        {
            AvailabilityId = availabilityId;
            ItineraryNumber = itineraryNumber;
            Nationality = nationality;
            PaymentMethod = paymentMethod;
            RejectIfUnavailable = rejectIfUnavailable;
            Residency = residency;
            AgentReference = agentReference;
            AgreementId = agreementId;

            RoomDetails = roomDetails ?? new List<BookingRoomDetails>(0);
            Features = features ?? new List<AccommodationFeature>(0);
        }

        /// <summary>
        ///     Availability ID obtained from an <see cref="AvailabilityResponse" />.
        /// </summary>
        [Required]
        public int AvailabilityId { get; }

        /// <summary>
        ///     The check-in date.
        /// </summary>

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

        /// <summary>
        ///     The payment method for a booking.
        /// </summary>
        [Required]
        public PaymentMethods PaymentMethod { get; }

        /// <summary>
        ///     Room details from an availability response.
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }

        /// <summary>
        /// Free text, used by user to put additional info.
        /// </summary>
        public string AgentReference { get; }

        /// <summary>
        ///     The selected additional accommodation features, if any.
        /// </summary>
        public List<AccommodationFeature> Features { get; }

        /// <summary>
        /// Identifier of chosen agreement.
        /// </summary>
        public Guid AgreementId { get; }
        
        /// <summary>
        ///     The Itinerary number to combine several orders in one pack.
        /// </summary>
        public long? ItineraryNumber { get; }
    }
}