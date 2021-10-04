using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingConfirmationStatuses
    {
        OnRequest = 1,
        Amended = 2,
        Confirmed = 3,
        Cancelled = 4
     }
}
