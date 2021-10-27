using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct AvailabilityRequest
    {
        [JsonConstructor]
        public AvailabilityRequest(string nationality, string residency, DateTime checkInDate, DateTime checkOutDate,
            ClientSearchFilters filters, List<RoomOccupationRequest> roomDetails, AccommodationRatings ratings, 
            PropertyTypes propertyTypes = default, List<string> htIds = null)
        {
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Filters = filters;
            Nationality = nationality;
            PropertyType = propertyTypes;
            Ratings = ratings;
            Residency = residency;
            RoomDetails = roomDetails;
            HtIds = htIds;
        }


        /// <summary>
        ///     Required. Check-in date.
        /// </summary>
        [Required]
        public DateTime CheckInDate { get; }

        /// <summary>
        ///     Required. Check-out date.
        /// </summary>
        [Required]
        public DateTime CheckOutDate { get; }

        /// <summary>
        ///     One ore several filters to order a response data.
        /// </summary>
        public ClientSearchFilters Filters { get; }


        /// <summary>
        ///     Required. Alpha-2 nationality code for a lead passengers.
        /// </summary>
        [Required]
        public string Nationality { get; }

        /// <summary>
        ///     Desirable property type for an accommodation.
        /// </summary>
        public PropertyTypes PropertyType { get; }

        /// <summary>
        ///     Accommodation rating.
        /// </summary>
        public AccommodationRatings Ratings { get; }

        /// <summary>
        ///     Required. Alpha-2 residency code for a lead passengers.
        /// </summary>
        [Required]
        public string Residency { get; }

        /// <summary>
        ///     Required. Desirable room details.
        /// </summary>
        [Required]
        public List<RoomOccupationRequest> RoomDetails { get; }

        /// <summary>
        /// Prediction's HtIds
        /// </summary>
        [Required]
        public List<string> HtIds { get; }
    }
}