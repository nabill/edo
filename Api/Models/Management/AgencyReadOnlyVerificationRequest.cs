using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct AgencyReadOnlyVerificationRequest
    {
        [JsonConstructor]
        public AgencyReadOnlyVerificationRequest(string reason)
        {
            Reason = reason;
        }


        /// <summary>
        ///     Verify reason.
        /// </summary>
        [Required]
        public string Reason { get; }
    }
}