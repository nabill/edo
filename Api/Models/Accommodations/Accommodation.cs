using System;
using System.Collections.Generic;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using HappyTravel.MapperContracts.Public.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct Accommodation
    {
        [JsonConstructor]
        public Accommodation(string id, string name, List<string> accommodationAmenities, Dictionary<string, string> additionalInfo,
            string category, in ContactInfo contacts, in LocationInfo location, List<ImageInfo> photos, AccommodationRatings rating,
            in ScheduleInfo schedule, List<TextualDescription> textualDescriptions, PropertyTypes type, List<SupplierInfo> suppliers,
            string htId = "", UniqueAccommodationCodes? uniqueCodes = null,  string hotelChain = null, DateTime? modified = null)
        {
            Id = id;
            AccommodationAmenities = accommodationAmenities ?? new List<string>(0);
            AdditionalInfo = additionalInfo ?? new Dictionary<string, string>(0);
            Category = category;
            Contacts = contacts;
            Rating = rating;
            Location = location;
            Name = name;
            Photos = photos ?? new List<ImageInfo>(0);
            Schedule = schedule;
            TextualDescriptions = textualDescriptions ?? new List<TextualDescription>(0);
            Type = type;
            Suppliers = suppliers;
            HtId = htId;
            UniqueCodes = uniqueCodes;
            HotelChain = hotelChain;
            Modified = modified;
        }


        /// <summary>
        ///     The accommodation ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     The accommodation name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The accommodation category.
        /// </summary>
        public string Category { get; }

        /// <summary>
        ///     Contact info.
        /// </summary>
        public ContactInfo Contacts { get; }

        /// <summary>
        ///     Location info.
        /// </summary>
        public LocationInfo Location { get; }

        /// <summary>
        ///     Accommodation pictures and their descriptions.
        /// </summary>
        public List<ImageInfo> Photos { get; }

        /// <summary>
        ///     The accommodation rating.
        /// </summary>
        public AccommodationRatings Rating { get; }

        /// <summary>
        ///     The schedule of accommodation services.
        /// </summary>
        public ScheduleInfo Schedule { get; }

        /// <summary>
        ///     Textual descriptions of an accommodation and its zones.
        /// </summary>
        public List<TextualDescription> TextualDescriptions { get; }

        /// <summary>
        ///     The type of a property.
        /// </summary>
        public PropertyTypes Type { get; }

        /// <summary>
        ///     Unique codes in external accommodation mapping services.
        /// </summary>
        public UniqueAccommodationCodes? UniqueCodes { get; }

        /// <summary>
        ///     Name of the hotel chain, where the hotel belongs to (Radisson, Hilton etc.)
        /// </summary>
        public string HotelChain { get; }

        /// <summary>
        ///     The Happytravel.com unique accommodation correlation ID.
        /// </summary>
        public string HtId { get; }

        /// <summary>
        ///     The dictionary of amenities available in an accommodation.
        /// </summary>
        public List<string> AccommodationAmenities { get; }

        /// <summary>
        ///     The dictionary of all other accommodation stats.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; }

        /// <summary>
        ///     The Modification date of accommodation data
        /// </summary>
        public DateTime? Modified { get; }

        /// <summary>
        ///     List of suppliers
        /// </summary>
        public List<SupplierInfo> Suppliers { get; }
    }
}