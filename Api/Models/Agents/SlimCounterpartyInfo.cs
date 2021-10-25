using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct SlimCounterpartyInfo
    {
        [JsonConstructor]
        public SlimCounterpartyInfo(int id, string name, string legalAddress, PaymentTypes preferredPaymentMethod,
            bool isContractUploaded, bool isActive,
            string markupFormula = null)
        {
            Id = id;
            Name = name;
            LegalAddress = legalAddress;
            PreferredPaymentMethod = preferredPaymentMethod;
            IsContractUploaded = isContractUploaded;
            MarkupFormula = markupFormula;
            IsActive = isActive;
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
        ///     Agency address.
        /// </summary>
        [Required]
        public string LegalAddress { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        public PaymentTypes PreferredPaymentMethod { get; }

        /// <summary>
        /// True if contract is loaded to counterparty
        /// </summary>
        public bool IsContractUploaded { get; }

        /// <summary>
        /// Displayed markup formula
        /// </summary>
        public string MarkupFormula { get; }

        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; }
    }
}