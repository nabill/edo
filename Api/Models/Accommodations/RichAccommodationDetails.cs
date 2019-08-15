using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RichAccommodationDetails
    {
        [JsonConstructor]
        public RichAccommodationDetails(in AccommodationDetails details, string description, string category, in RichLocationInfo location, List<TextualDescription> textualDescriptions)
        {
            Id = details.Id;
            Name = details.Name;
            Description = description;
            Rating = details.Rating;
            Category = category;
            Contacts = details.Contacts;
            Location = location;
            Pictures = details.Pictures;
            Schedule = details.Schedule;
            TextualDescriptions = textualDescriptions;
            AccommodationAmenities = details.AccommodationAmenities;
            RoomAmenities = details.RoomFacilities;
            AdditionalInfo = details.AdditionalInfo;
        }


        /// <summary>
        /// Accommodation ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Accommodation name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Accommodation category
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Contact info
        /// </summary>
        public ContactInfo Contacts { get; }

        /// <summary>
        /// Description of an accommodation style
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Location info
        /// </summary>
        public RichLocationInfo Location { get; }

        /// <summary>
        /// Accommodation pictures and their descriptions
        /// </summary>
        public List<Picture> Pictures { get; }

        /// <summary>
        /// Accommodation rating
        /// </summary>
        public AccommodationRatings Rating { get; }

        /// <summary>
        /// Schedule of accommodation services
        /// </summary>
        public ScheduleInfo Schedule { get; }

        /// <summary>
        /// Textual descriptions of an accommodation and its zones
        /// </summary>
        public List<TextualDescription> TextualDescriptions { get; }
        
        /// <summary>
        /// Dictionary of amenities available in an accommodation
        /// </summary>
        public Dictionary<string, bool> AccommodationAmenities { get; }
        
        /// <summary>
        /// Dictionary of amenities available in rooms
        /// </summary>
        public Dictionary<string, bool> RoomAmenities { get; }

        /// <summary>
        /// Dictionary of all other accommodation stats 
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; }
    }
}
