using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Agents;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct CounterpartyFullAccessVerificationRequest
    {
        [JsonConstructor]
        public CounterpartyFullAccessVerificationRequest(CounterpartyContractType contractType, string reason)
        {
            ContractType = contractType;
            Reason = reason;
        }


        /// <summary>
        /// Contract type
        /// </summary>
        [Required]
        public CounterpartyContractType ContractType { get; }

        /// <summary>
        /// Verify reason.
        /// </summary>
        [Required]
        public string Reason { get; }
    }
}