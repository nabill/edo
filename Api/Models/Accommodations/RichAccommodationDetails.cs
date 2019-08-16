using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RichAccommodationDetails
    {
        [JsonConstructor]
        public RichAccommodationDetails(string id, string name, Dictionary<string, bool> accommodationAmenities, Dictionary<string, string> additionalInfo,
            string category, ContactInfo contacts, string description, in RichLocationInfo location, List<Picture> pictures, AccommodationRatings rating,
            Dictionary<string, bool> roomAmenities, ScheduleInfo schedule, List<TextualDescription> textualDescriptions)
        {
            Id = id;
            Name = name;
            Category = category;
            Contacts = contacts;
            Description = description;
            Rating = rating;
            Location = location;
            Pictures = pictures;
            Schedule = schedule;
            TextualDescriptions = textualDescriptions;
            AccommodationAmenities = accommodationAmenities;
            RoomAmenities = roomAmenities;
            AdditionalInfo = additionalInfo;
        }


        /// <summary>
        ///     Accommodation ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Accommodation name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Accommodation category
        /// </summary>
        public string Category { get; }

        /// <summary>
        ///     Contact info
        /// </summary>
        public ContactInfo Contacts { get; }

        /// <summary>
        ///     Description of an accommodation style
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Location info
        /// </summary>
        public RichLocationInfo Location { get; }

        /// <summary>
        ///     Accommodation pictures and their descriptions
        /// </summary>
        public List<Picture> Pictures { get; }

        /// <summary>
        ///     Accommodation rating
        /// </summary>
        public AccommodationRatings Rating { get; }

        /// <summary>
        ///     Schedule of accommodation services
        /// </summary>
        public ScheduleInfo Schedule { get; }

        /// <summary>
        ///     Textual descriptions of an accommodation and its zones
        /// </summary>
        public List<TextualDescription> TextualDescriptions { get; }

        /// <summary>
        ///     Dictionary of amenities available in an accommodation
        /// </summary>
        public Dictionary<string, bool> AccommodationAmenities { get; }

        /// <summary>
        ///     Dictionary of amenities available in rooms
        /// </summary>
        public Dictionary<string, bool> RoomAmenities { get; }

        /// <summary>
        ///     Dictionary of all other accommodation stats
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; }
    }
}