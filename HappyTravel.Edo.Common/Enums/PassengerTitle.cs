using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PassengerTitle
    {
        Unspecified = 0,
        MISS = 1,
        MR = 2,
        MRS = 3,
        MS = 4
    }
}
