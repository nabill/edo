using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimAccommodationDetails
    {
        [JsonConstructor]
        public SlimAccommodationDetails(string id, Dictionary<string, bool> accommodationAmenities, Dictionary<string, string> additionalInfo,
            List<AccommodationFeatureInfo> features, SlimLocationInfo location, string name, Picture picture,
            AccommodationRatings rating, Dictionary<string, bool> roomAmenities, TextualDescription textualDescription)
        {
            Id = id;
            AccommodationAmenities = accommodationAmenities;
            AdditionalInfo = additionalInfo;
            Features = features;
            GeneralTextualDescription = textualDescription;
            Location = location;
            Name = name;
            Picture = picture;
            Rating = rating;
            RoomAmenities = roomAmenities;
        }


        public string Id { get; }
        public TextualDescription GeneralTextualDescription { get; }
        public SlimLocationInfo Location { get; }
        public string Name { get; }
        public Picture Picture { get; }
        public AccommodationRatings Rating { get; }
        public Dictionary<string, bool> AccommodationAmenities { get; }
        public Dictionary<string, string> AdditionalInfo { get; }
        public List<AccommodationFeatureInfo> Features { get; }
        public Dictionary<string, bool> RoomAmenities { get; }
    }
}