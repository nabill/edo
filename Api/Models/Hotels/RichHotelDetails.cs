using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct RichHotelDetails
    {
        [JsonConstructor]
        public RichHotelDetails(in HotelDetails details, string description, string category, in RichLocationInfo location, List<TextualDescription> textualDescriptions)
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
            HotelAmenities = details.HotelFacilities;
            RoomAmenities = details.RoomFacilities;
            AdditionalInfo = details.AdditionalInfo;
        }


        /// <summary>
        /// Hotel ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Hotel name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Hotel category
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Contact info
        /// </summary>
        public ContactInfo Contacts { get; }

        /// <summary>
        /// Description of a hotel style
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Location info
        /// </summary>
        public RichLocationInfo Location { get; }

        /// <summary>
        /// Hotel pictures and their descriptions
        /// </summary>
        public List<Picture> Pictures { get; }

        /// <summary>
        /// Hotel rating
        /// </summary>
        public HotelRatings Rating { get; }

        /// <summary>
        /// Schedule of hotel services
        /// </summary>
        public ScheduleInfo Schedule { get; }

        /// <summary>
        /// Textual descriptions of the hotel and its zones
        /// </summary>
        public List<TextualDescription> TextualDescriptions { get; }
        
        /// <summary>
        /// Dictionary of amenities available in the hotel
        /// </summary>
        public Dictionary<string, bool> HotelAmenities { get; }
        
        /// <summary>
        /// Dictionary of amenities available in rooms
        /// </summary>
        public Dictionary<string, bool> RoomAmenities { get; }

        /// <summary>
        /// Dictionary of all other hotel stats 
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; }
    }
}
