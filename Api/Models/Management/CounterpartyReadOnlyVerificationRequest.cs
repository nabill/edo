using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyReadOnlyVerificationRequest
    {
        [JsonConstructor]
        public CounterpartyReadOnlyVerificationRequest(string reason)
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