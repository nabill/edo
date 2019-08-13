using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Models.Hotels;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct HotelBookingRequest
    {
        [JsonConstructor]
        public HotelBookingRequest(string availabilityId, string nationality, string residency,
            DateTime checkInDate, DateTime checkOutDate, string hotelId, string tariffCode,
            List<BookingRoomDetails> roomDetails, List<HotelFeature> features, bool availableOnly = true)
        {
            AvailabilityId = availabilityId;
            AvailableOnly = availableOnly;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            HotelId = hotelId;
            Nationality = nationality;
            Residency = residency;
            TariffCode = tariffCode;

            RoomDetails = roomDetails ?? new List<BookingRoomDetails>(0);
            Features = features ?? new List<HotelFeature>(0);
        }


        /// <summary>
        ///     Availability ID obtained from an <see cref="AvailabilityResponse" />.
        /// </summary>
        [Required]
        public string AvailabilityId { get; }

        /// <summary>
        ///     This indicates the system to reject the request when an accommodation has been booked by some one else between
        ///     availability and booking requests. Default is true.
        /// </summary>
        public bool AvailableOnly { get; }

        /// <summary>
        ///     The check-in date.
        /// </summary>
        [Required]
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     The check-out date.
        /// </summary>
        [Required]
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     The ID of a booked accommodation.
        /// </summary>
        [Required]
        public string HotelId { get; }

        /// <summary>
        ///     The nationality of a main passenger.
        /// </summary>
        [Required]
        public string Nationality { get; }

        /// <summary>
        ///     The residency of a main passenger.
        /// </summary>
        [Required]
        public string Residency { get; }

        /// <summary>
        ///     Tariff code from an agreements section of availability response.
        /// </summary>
        [Required]
        public string TariffCode { get; }

        /// <summary>
        ///     Room details from an availability response.
        /// </summary>
        public List<BookingRoomDetails> RoomDetails { get; }

        /// <summary>
        ///     The selected additional accommodation features, if any.
        /// </summary>
        public List<HotelFeature> Features { get; }
    }
}