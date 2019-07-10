using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HotelRatings
    {
        Unknown = 1,
        NotRated = 2,
        OneStar = 4,
        TwoStars = 8,
        ThreeStars = 16,
        FourStars = 32,
        FiveStars = 64
    }
}
