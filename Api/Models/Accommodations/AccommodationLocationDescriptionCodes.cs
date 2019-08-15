using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccommodationLocationDescriptionCodes
    {
        Unspecified = 0,
        CityCenter = 1,
        Airport = 2,
        RailwayStation = 3,
        Port = 4,
        SeaOrBeach = 5,
        OpenCountry = 6,
        Mountains = 7,
        Peripherals = 8,
        CloseToCityCentre = 9
    }
}
