using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimAccommodationDetails
    {
        [JsonConstructor]
        public SlimAccommodationDetails(AccommodationDetails details, List<AccommodationFeatureInfo> features, SlimLocationInfo location, Picture picture, TextualDescription textualDescription)
        {
            Id = details.Id;
            Features = features;
            GeneralTextualDescription = textualDescription;
            Location = location;
            Name = details.Name;
            Picture = picture;
            Rating = details.Rating;
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
