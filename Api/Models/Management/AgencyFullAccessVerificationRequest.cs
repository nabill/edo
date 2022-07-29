using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct AgencyFullAccessVerificationRequest
    {
        [JsonConstructor]
        public AgencyFullAccessVerificationRequest(ContractKind contractKind,
            string reason, MoneyAmount? creditLimit)
        {
            ContractKind = contractKind;
            Reason = reason;
            CreditLimit = creditLimit;
        }


        /// <summary>
        /// Contract type
        /// </summary>
        [Required]
        public ContractKind ContractKind { get; }

        /// <summary>
        /// Verify reason.
        /// </summary>
        [Required]
        public string Reason { get; }

        /// <summary>
        /// Credit limit required when Contract type equals VirtualAccountOrCreditCardPayments.
        /// </summary>
        public MoneyAmount? CreditLimit { get; }
    }
}