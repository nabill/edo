using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeInitiators
    {
        None = 0,
        Administrator = 1,
        Agent = 2,
        Supplier = 3,
        System = 4,
    }
}
