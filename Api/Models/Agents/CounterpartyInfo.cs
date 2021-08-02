using System;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(int id, string name, string address, string billingEmail, string city, string countryCode, string countryName, 
            string fax, string phone, string postalCode, string website, string vatNumber, PaymentTypes preferredPaymentMethod,
            bool isContractUploaded, CounterpartyStates verificationState, DateTime? verificationDate, bool isActive,
            string markupFormula = null)
        {
            Id = id;
            Name = name;
            Address = address;
            BillingEmail = billingEmail;
            City = city;
            CountryCode = countryCode;
            CountryName = countryName;
            Fax = fax;
            Phone = phone;
            PostalCode = postalCode;
            Website = website;
            VatNumber = vatNumber;
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
        ///     Counterparty address.
        /// </summary>
        [Required]
        public string Address { get; }

        /// <summary>
        ///     Two-letter international country code.
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        /// Country name.
        /// </summary>
        public string CountryName { get; }

        /// <summary>
        ///     City name.
        /// </summary>
        [Required]
        public string City { get; }

        /// <summary>
        ///     Phone number. Only digits, length between 3 and 30.
        /// </summary>
        [Required]
        public string Phone { get; }

        /// <summary>
        ///     Fax number. Only digits, length between 3 and 30.
        /// </summary>
        public string Fax { get; }

        /// <summary>
        ///     Postal code.
        /// </summary>
        public string PostalCode { get; }

        /// <summary>
        ///     Counterparty site url.
        /// </summary>
        public string Website { get; }

        /// <summary>
        /// E-mail for billing operations
        /// </summary>
        public string BillingEmail { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string VatNumber { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        public PaymentTypes PreferredPaymentMethod { get; }

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