using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct AgencyFullAccessVerificationRequest
    {
        [JsonConstructor]
        public AgencyFullAccessVerificationRequest(int? agencyId, ContractKind contractKind,
            string reason, MoneyAmount? creditLimit)
        {
            AgencyId = agencyId;
            ContractKind = contractKind;
            Reason = reason;
            CreditLimit = creditLimit;
        }


        public AgencyFullAccessVerificationRequest(int? agencyId, AgencyFullAccessVerificationRequest request)
            : this(agencyId, request.ContractKind, request.Reason, request.CreditLimit)
        { }


        /// <summary>
        /// AgencyId
        /// </summary>
        public int? AgencyId { get; }

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