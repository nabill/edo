using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ManualBookingRejectionRequest
    {
        /// <summary>
        /// Rejection reason
        /// </summary>
        [Required]
        public string Reason { get; init; }
    }
}