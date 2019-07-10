using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Hotels;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct AvailabilityRequest
    {
        [JsonConstructor]
        public AvailabilityRequest(string nationality, string residency, DateTime checkInDate, DateTime checkOutDate, 
            SearchFilters filters, List<RoomDetails> roomDetails, List<string> hotelIds, HotelRatings ratings, 
            SearchLocation location = default, PropertyTypes propertyTypes = default)
        {
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Filters = filters;
            HotelIds = hotelIds ?? new List<string>();
            Location = location;
            Nationality = nationality;
            PropertyTypes = propertyTypes;
            Ratings = ratings;
            Residency = residency;
            RoomDetails = roomDetails ?? new List<RoomDetails>();
        }

        
        /// <summary>
        /// Required. Check-in date.
        /// </summary>
        [Required]
        public DateTime CheckInDate { get; }

        /// <summary>
        /// Required. Check-out date.
        /// </summary>
        [Required]
        public DateTime CheckOutDate { get; }

        /// <summary>
        /// One ore several filters to order a response data.
        /// </summary>
        public SearchFilters Filters { get; }

        /// <summary>
        /// Required, but can be empty. List of desirable hotel IDs.
        /// </summary>
        [Required]
        public List<string> HotelIds { get; }

        /// <summary>
        /// Desirable search area.
        /// </summary>
        public SearchLocation Location { get; }

        /// <summary>
        /// Required. Alpha-2 nationality code for a lead passengers.
        /// </summary>
        [Required]
        public string Nationality { get; }

        /// <summary>
        /// Desirable property type for an accommodation.
        /// </summary>
        public PropertyTypes PropertyTypes { get; }

        /// <summary>
        /// Hotel rating.
        /// </summary>
        public HotelRatings Ratings { get; }

        /// <summary>
        /// Required. Alpha-2 residency code for a lead passengers.
        /// </summary>
        [Required]
        public string Residency { get; }

        /// <summary>
        /// Required. Desirable room details.
        /// </summary>
        [Required]
        public List<RoomDetails> RoomDetails { get; }
    }
}
