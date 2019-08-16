using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct AccommodationDetails
    {
        [JsonConstructor]
        public AccommodationDetails(string id, string name, string descriptionCode, AccommodationRatings rating, string categoryCode, LocationInfo location,
            ContactInfo contacts, List<Picture> pictures, ScheduleInfo schedule, List<TextualDescription> textualDescriptions, 
            Dictionary<string, bool> accommodationAmenities, Dictionary<string, bool> roomFacilities, 
            Dictionary<string, string> additionalInfo, List<RestaurantInfo> restaurants)
        {
            Id = id;
            AdditionalInfo = additionalInfo;
            CategoryCode = categoryCode;
            Contacts = contacts;
            DescriptionCode = descriptionCode;
            AccommodationAmenities = accommodationAmenities;
            Location = location;
            Name = name;
            RoomFacilities = roomFacilities;
            Pictures = pictures;
            Rating = rating;
            Restaurants = restaurants;
            Schedule = schedule;
            TextualDescriptions = textualDescriptions;
        }


        public string Id { get; }
        public string Name { get; }
        public string CategoryCode { get; }
        public ContactInfo Contacts { get; }
        public string DescriptionCode { get; }
        public LocationInfo Location { get; }
        public List<Picture> Pictures { get; }
        public AccommodationRatings Rating { get; }
        public List<RestaurantInfo> Restaurants { get; }
        public ScheduleInfo Schedule { get; }
        public List<TextualDescription> TextualDescriptions { get; }
        public Dictionary<string, bool> AccommodationAmenities { get; }
        public Dictionary<string, bool> RoomFacilities { get; }
        public Dictionary<string, string> AdditionalInfo { get; }
    }
}
