using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PassengerTitle
    {
        Unspecified,
        MISS,
        MR,
        MRS,
        MS
    }
}
