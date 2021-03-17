using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingChangeEvents
    {
        None = 0,
        Discarded = 1,
        CanceledManually = 2,
        RejectedManually = 3,
        ConfirmManually = 4,
        Charge = 5,
        Finalize = 6,
        Cancel = 7
    }
}
