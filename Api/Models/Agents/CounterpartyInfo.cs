using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(int id, string name, string legalAddress, PaymentMethods preferredPaymentMethod,
            bool isContractUploaded, CounterpartyStates verificationState, DateTime? verificationDate, bool isActive,
            string markupFormula = null)
        {
            Id = id;
            Name = name;
            LegalAddress = legalAddress;
            PreferredPaymentMethod = preferredPaymentMethod;
            IsContractUploaded = isContractUploaded;
            VerificationState = verificationState;
            MarkupFormula = markupFormula;
            VerificationDate = verificationDate;
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
        public PaymentMethods PreferredPaymentMethod { get; }

        /// <summary>
        /// True if contract is loaded to counterparty
        /// </summary>
        public bool IsContractUploaded { get; }

        /// <summary>
        /// Verification state of the counterparty
        /// </summary>
        public CounterpartyStates VerificationState { get; }


        /// <summary>
        /// Displayed markup formula
        /// </summary>
        public string MarkupFormula { get; }

        /// <summary>
        /// Counterparty verification date
        /// </summary>
        public DateTime? VerificationDate { get; }

        /// <summary>
        /// Activity state
        /// </summary>
        public bool IsActive { get; }
    }
}