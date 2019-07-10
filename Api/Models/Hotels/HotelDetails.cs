using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct HotelDetails
    {
        [JsonConstructor]
        public HotelDetails(string id, string name, string descriptionCode, HotelRatings rating, string categoryCode, LocationInfo location,
            ContactInfo contacts, List<Picture> pictures, ScheduleInfo schedule, List<TextualDescription> textualDescriptions, 
            Dictionary<string, bool> hotelFacilities, Dictionary<string, bool> roomFacilities, 
            Dictionary<string, string> additionalInfo, List<RestaurantInfo> restaurants)
        {
            Id = id;
            AdditionalInfo = additionalInfo;
            CategoryCode = categoryCode;
            Contacts = contacts;
            DescriptionCode = descriptionCode;
            HotelFacilities = hotelFacilities;
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
        public HotelRatings Rating { get; }
        public List<RestaurantInfo> Restaurants { get; }
        public ScheduleInfo Schedule { get; }
        public List<TextualDescription> TextualDescriptions { get; }
        public Dictionary<string, bool> HotelFacilities { get; }
        public Dictionary<string, bool> RoomFacilities { get; }
        public Dictionary<string, string> AdditionalInfo { get; }
    }
}
