using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeReasons
    {
        ChangedByAgent = 0,
        ChangedByAdministrator = 1,
        ReceivedFromSupplier = 2
    }
}
