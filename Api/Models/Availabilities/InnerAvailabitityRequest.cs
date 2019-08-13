using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Hotels;
using HappyTravel.Edo.Api.Models.Locations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public class InnerAvailabilityRequest
    {
        [JsonConstructor]
        public InnerAvailabilityRequest(string nationality, string residency, DateTime checkInDate, DateTime checkOutDate,
            SearchFilters filters, List<RoomDetails> roomDetails, List<string> hotelIds, Location location, PropertyTypes propertyTypes, HotelRatings ratings)
        {
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Filters = filters;
            HotelIds = hotelIds;
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
            HotelIds = request.AccommodationIds;
            Nationality = request.Nationality;
            PropertyTypes = request.PropertyTypes;
            Ratings = request.Ratings;
            Residency = request.Residency;
            RoomDetails = request.RoomDetails;

            Location = location;
        }

        
        [Required]
        public DateTime CheckInDate { get; }

        [Required]
        public DateTime CheckOutDate { get; }

        public SearchFilters Filters { get; }

        public Location Location { get; }

        [Required]
        public List<string> HotelIds { get; }

        [Required]
        public string Nationality { get; }

        public PropertyTypes PropertyTypes { get; }

        public HotelRatings Ratings { get; }

        [Required]
        public string Residency { get; }

        [Required]
        public List<RoomDetails> RoomDetails { get; }
    }
}
