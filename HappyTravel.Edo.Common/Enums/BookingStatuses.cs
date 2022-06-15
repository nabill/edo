using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BookingStatuses
    {
        // The status with which booking is created. If booking exists in this status for a long time this means that it 
        // is not properly created (not paid by credit card, not met some validation etc.) and can be treated as not existing
        Created = 0,
        
        /// <summary>
        /// Waiting for supplier updates, typically for async web hooks
        /// </summary>
        WaitingForResponse = 1,
        
        /// <summary>
        /// Waiting for status update, typically for sync bookings
        /// </summary>
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
        
        /// <summary>
        /// This status means that booking is not finished properly and can be treated as not existing
        /// </summary>
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
        PendingCancellation = 9,
        
        /// <summary>
        /// The booking has been confirmed and paid. 24 hours since a check-out date have passed
        /// </summary>
        Completed = 10
    }
}