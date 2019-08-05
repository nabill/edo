using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    public readonly struct SlimHotelDetails
    {
        [JsonConstructor]
        public SlimHotelDetails(HotelDetails details, SlimLocationInfo location, Picture picture, TextualDescription textualDescription)
        {
            Id = details.Id;
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
    }
}
