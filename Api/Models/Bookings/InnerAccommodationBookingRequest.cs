using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct InnerAccommodationBookingRequest
    {
        [JsonConstructor]
        public InnerAccommodationBookingRequest(in AccommodationBookingRequest request, 
            in BookingAvailabilityInfo bookingAvailabilityInfo,
            string referenceCode)
        {
            AccommodationId = bookingAvailabilityInfo.AccommodationId.ToString();
            AvailabilityId = request.AvailabilityId.ToString();
            CheckInDate = bookingAvailabilityInfo.CheckInDate;
            CheckOutDate = bookingAvailabilityInfo.CheckOutDate;
            Nationality = request.Nationality;
            PaymentMethod = request.PaymentMethod;
            RejectIfUnavailable = request.RejectIfUnavailable;
            Residency = request.Residency;
            TariffCode = bookingAvailabilityInfo.Agreement.TariffCode;

            RoomDetails = request.RoomDetails;
            Features = request.Features;

            ReferenceCode = referenceCode;
        }

        public string AccommodationId { get; }

        public string AvailabilityId { get; }

        public DateTime CheckInDate { get; }

        public DateTime CheckOutDate { get; }

        public string Nationality { get; }

        public string Residency { get; }

        public PaymentMethods PaymentMethod { get; }

        public string ReferenceCode { get; }

        public bool RejectIfUnavailable { get; }

        public List<BookingRoomDetails> RoomDetails { get; }

        public string TariffCode { get; }

        public List<AccommodationFeature> Features { get; }
    }
}
