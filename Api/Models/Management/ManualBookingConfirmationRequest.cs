using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ManualBookingConfirmationRequest
    {
        /// <summary>
        /// Date when booking was actually confirmed
        /// </summary>
        [Required]
        public DateTime ConfirmationDate { get; init; }
        
        /// <summary>
        /// Confirmation reason
        /// </summary>
        [Required]
        public string Reason { get; init; }
    }
}