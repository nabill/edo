using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeReasons
    {
        None = 0,
        ReceivedFromSupplier = 1,
        ChangedByAdministrator = 2
    }
}
