using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyDeclinedVerificationRequest
    {
        [JsonConstructor]
        public CounterpartyDeclinedVerificationRequest(string reason)
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