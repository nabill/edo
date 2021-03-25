using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(int id, string name, PaymentMethods preferredPaymentMethod,
            string vatNumber, bool isContractUploaded, string markupFormula = null)
        {
            Id = id;
            Name = name;
            PreferredPaymentMethod = preferredPaymentMethod;
            VatNumber = vatNumber;
            IsContractUploaded = isContractUploaded;
            MarkupFormula = markupFormula;
        }

        /// <summary>
        /// Counterparty Id.
        /// </summary>
        [Required]
        public int Id { get; }

        /// <summary>
        ///     Counterparty name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        public PaymentMethods PreferredPaymentMethod { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string VatNumber { get; }

        /// <summary>
        /// True if contract is loaded to counterparty
        /// </summary>
        public bool IsContractUploaded { get; }
        
        
        /// <summary>
        /// Displayed markup formula
        /// </summary>
        public string MarkupFormula { get; }
    }
}