using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeReasons
    {
        None = 0,
        ChangedBySystemAtAgentRequest = 1,
        ReceivedFromSupplier = 2,
        ChangedByAdministrator = 3
    }
}
