using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingStatuses
    {
        // TODO: Remove not needed statuses and migrate existing bookings statuses
        InternalProcessing = 0,
        WaitingForResponse = 1,
        Pending = 2,
        Confirmed = 3,
        Cancelled = 4,
        Rejected = 5,
        Invalid = 6,
        Reverted = 7,
        ManualCorrectionNeeded = 8
    }
}