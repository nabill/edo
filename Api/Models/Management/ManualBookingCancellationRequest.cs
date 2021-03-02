using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ManualBookingCancellationRequest
    {
        /// <summary>
        /// Date when booking was actually cancelled on supplier end. This value affects applied cancellation penalties
        /// </summary>
        [Required]
        public DateTime CancellationDate { get; init; }
        
        /// <summary>
        /// Cancellation reason
        /// </summary>
        [Required]
        public string Reason { get; init; }
    }
}