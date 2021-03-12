using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeEvents
    {
        None = 0,   // TODO: Need reorder later and set integer values for all
        Discarded,
        CanceledManually,
        RejectedManually
    }
}
