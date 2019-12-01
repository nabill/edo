using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public class InnerAvailabilityRequest
    {
        [JsonConstructor]
        public InnerAvailabilityRequest(string nationality, string residency, DateTime checkInDate, DateTime checkOutDate,
            SearchFilters filters, List<RoomDetails> roomDetails, List<string> accommodationIds, Location location, PropertyTypes propertyTypes, AccommodationRatings ratings)
        {
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Filters = filters;
            AccommodationIds = accommodationIds;
            Location = location;
            Nationality = nationality;
            PropertyTypes = propertyTypes;
            Ratings = ratings;
            Residency = residency;
            RoomDetails = roomDetails;
        }


        public InnerAvailabilityRequest(in AvailabilityRequest request, in Location location)
        {
            CheckInDate = request.CheckInDate;
            CheckOutDate = request.CheckOutDate;
            Filters = request.Filters;
            AccommodationIds = request.AccommodationIds;
            Nationality = request.Nationality;
            PropertyTypes = request.PropertyType;
            Ratings = request.Ratings;
            Residency = request.Residency;
            RoomDetails = request.RoomDetails;
            SearchInfo = request.SearchInfo;

            Location = location;
        }

        
        [Required]
        public List<string> AccommodationIds { get; }

        [Required]
        public DateTime CheckInDate { get; }

        [Required]
        public DateTime CheckOutDate { get; }

        public SearchFilters Filters { get; }

        public Location Location { get; }

        [Required]
        public string Nationality { get; }

        public PropertyTypes PropertyTypes { get; }

        public AccommodationRatings Ratings { get; }

        [Required]
        public string Residency { get; }

        [Required]
        public List<RoomDetails> RoomDetails { get; }

        public SearchInfo SearchInfo { get; }
    }
}
