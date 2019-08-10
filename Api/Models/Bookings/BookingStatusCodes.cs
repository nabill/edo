using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingStatusCodes
    {
        /// <summary>
        ///     If you see it for more than 20 minutes (it should be a status changing very very fast to something else, like CNF
        ///     or REJ),
        ///     that something went wrong. A manual intervention is required.
        /// </summary>
        InternalProcessing,
        WaitingForResponse,
        Pending,
        Confirmed,
        Cancelled,
        Rejected,
        Invalid
    }
}
