using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct Accommodation
    {
        [JsonConstructor]
        public Accommodation(string id, string name, List<string> accommodationAmenities, Dictionary<string, string>? additionalInfo,
            string? category, ContactInfo contacts, LocationInfo location, List<ImageInfo>? photos, AccommodationRatings rating,
            in ScheduleInfo schedule, List<TextualDescription>? textualDescriptions, PropertyTypes type,
            string? hotelChain, DateTime? modified)
        {
            Id = id;
            AccommodationAmenities = accommodationAmenities;
            AdditionalInfo = additionalInfo;
            Category = category;
            Contacts = contacts;
            Rating = rating;
            Location = location;
            Name = name;
            Photos = photos;
            Schedule = schedule;
            TextualDescriptions = textualDescriptions;
            Type = type;
            HotelChain = hotelChain;
            Modified = modified;
        }


        /// <summary>
        ///     ID for the accommodation
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Name of the accommodation
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Category of the accommodation
        /// </summary>
        public string? Category { get; }

        /// <summary>
        ///     Contact info
        /// </summary>
        public ContactInfo Contacts { get; }

        /// <summary>
        ///     Location info
        /// </summary>
        public LocationInfo Location { get; }

        /// <summary>
        ///     Accommodation pictures and their descriptions
        /// </summary>
        public List<ImageInfo>? Photos { get; }

        /// <summary>
        ///     Accommodation rating
        /// </summary>
        public AccommodationRatings Rating { get; }

        /// <summary>
        ///     Schedule of accommodation services
        /// </summary>
        public ScheduleInfo Schedule { get; }

        /// <summary>
        ///     Description of the accommodation or part of the accommodation
        /// </summary>
        public List<TextualDescription>? TextualDescriptions { get; }

        /// <summary>
        ///     Type of property
        /// </summary>
        public PropertyTypes Type { get; }

        /// <summary>
        ///     Name of the hotel chain (such as Radisson or Hilton)
        /// </summary>
        public string? HotelChain { get; }

        /// <summary>
        ///     List of amenities available at the accommodation
        /// </summary>
        public List<string> AccommodationAmenities { get; }

        /// <summary>
        ///     Dictionary of all other accommodation info
        /// </summary>
        public Dictionary<string, string>? AdditionalInfo { get; }

        /// <summary>
        ///     Date when the accommodation data was last modified
        /// </summary>
        public DateTime? Modified { get; }
    }
}