using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimAccommodationDetails
    {
        [JsonConstructor]
        public SlimAccommodationDetails(string id, List<AccommodationFeatureInfo> features, SlimLocationInfo location, string name, Picture picture,
            AccommodationRatings rating, TextualDescription textualDescription)
        {
            Id = id;
            Features = features;
            GeneralTextualDescription = textualDescription;
            Location = location;
            Name = name;
            Picture = picture;
            Rating = rating;
        }


        public string Id { get; }
        public TextualDescription GeneralTextualDescription { get; }
        public SlimLocationInfo Location { get; }
        public string Name { get; }
        public Picture Picture { get; }
        public AccommodationRatings Rating { get; }
        public List<AccommodationFeatureInfo> Features { get; }
    }
}