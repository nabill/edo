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
        
        /// <summary>
        /// Confirmed by supplier
        /// </summary>
        Confirmed = 3,
        
        /// <summary>
        /// Cancelled by supplier
        /// </summary>
        Cancelled = 4,
        
        /// <summary>
        /// Rejected by supplier
        /// </summary>
        Rejected = 5,
        Invalid = 6,
        
        /// <summary>
        /// Discarded by administrator
        /// </summary>
        Discarded = 7,
        
        /// <summary>
        /// Needs a manual status change (by administrator)
        /// </summary>
        ManualCorrectionNeeded = 8,
        
        /// <summary>
        /// Sent cancellation request, waiting for async response
        /// </summary>
        PendingCancellation = 9
    }
}