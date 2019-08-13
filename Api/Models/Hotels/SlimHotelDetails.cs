using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct SlimHotelDetails
    {
        [JsonConstructor]
        public SlimHotelDetails(HotelDetails details, List<HotelFeatureInfo> features, SlimLocationInfo location, Picture picture, TextualDescription textualDescription)
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
        public HotelRatings Rating { get; }
        public List<HotelFeatureInfo> Features { get; }
    }
}
