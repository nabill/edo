using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyEditRequest
    {
        [JsonConstructor]
        public CounterpartyEditRequest(string name, PaymentMethods preferredPaymentMethod, string vatNumber)
        {
            Name = name;
            PreferredPaymentMethod = preferredPaymentMethod;
            VatNumber = vatNumber;
        }


        /// <summary>
        ///     Counterparty name.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        [Required]
        public PaymentMethods PreferredPaymentMethod { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string VatNumber { get; }
    }
}