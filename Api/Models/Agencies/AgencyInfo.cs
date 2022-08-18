using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct AgencyInfo
    {
        [JsonConstructor]
        public AgencyInfo(string name, int? id, string address, string billingEmail, string city,
            string countryCode, string countryName, string fax, string phone, string postalCode, string website, string vatNumber,
            PaymentTypes defaultPaymentType, string countryHtId, string localityHtId, List<int> ancestors,
            AgencyVerificationStates verificationState, DateTime? verificationDate, bool isActive, string legalAddress, PaymentTypes preferredPaymentMethod,
            bool isContractUploaded, string markupDisplayFormula, Currencies preferredCurrency, string? accountManagerName, int? accountManagerId,
            ContractKind? contractKind, MoneyAmount? creditLimit, string? taxRegistrationNumber)
        {
            Name = name;
            Id = id;
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
            DefaultPaymentType = defaultPaymentType;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            Ancestors = ancestors;
            VerificationState = verificationState;
            VerificationDate = verificationDate;
            IsActive = isActive;
            LegalAddress = legalAddress;
            PreferredPaymentMethod = preferredPaymentMethod;
            IsContractUploaded = isContractUploaded;
            MarkupDisplayFormula = markupDisplayFormula;
            PreferredCurrency = preferredCurrency;
            AccountManagerName = accountManagerName;
            AccountManagerId = accountManagerId;
            ContractKind = contractKind;
            CreditLimit = creditLimit;
            TaxRegistrationNumber = taxRegistrationNumber;
        }


        /// <summary>
        ///     Name of the agency.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        ///     Id of the agency.
        /// </summary>
        public int? Id { get; }

        /// <summary>
        ///     Agency address.
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
        ///     Agency site url.
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
        /// Default payment type
        /// </summary>
        public PaymentTypes DefaultPaymentType { get; }

        /// <summary>
        /// List of ancestors ids
        /// </summary>
        public List<int> Ancestors { get; }

        /// <summary>
        /// Country of agency
        /// </summary>
        public string CountryHtId { get; }

        /// <summary>
        /// Locality of agency
        /// </summary>
        public string LocalityHtId { get; }

        /// <summary>
        /// Verification state of the agency
        /// </summary>
        public AgencyVerificationStates VerificationState { get; }

        /// <summary>
        /// Agency verification date
        /// </summary>
        public DateTime? VerificationDate { get; }

        /// <summary>
        /// Activity status
        /// </summary>
        public bool IsActive { get; }

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
        /// True if contract is loaded to agency
        /// </summary>
        public bool IsContractUploaded { get; }

        /// <summary>
        /// Calculated with all markups formula
        /// </summary>
        public string MarkupDisplayFormula { get; }

        /// <summary>
        /// Agency preferred currency
        /// </summary>
        public Currencies PreferredCurrency { get; }

        /// <summary>
        /// Account manager id
        /// </summary>
        public int? AccountManagerId { get; }

        /// <summary>
        /// Name of the account manager
        /// </summary>
        public string? AccountManagerName { get; }

        /// <summary>
        /// Contract kind of the agency
        /// </summary>
        public ContractKind? ContractKind { get; }

        /// <summary>
        /// Agencies credit limit
        /// </summary>
        public MoneyAmount? CreditLimit { get; }

        /// <summary>
        /// Tax registration number(For UAE)
        /// </summary>
        public string? TaxRegistrationNumber { get; }


        public override int GetHashCode()
            => (Name, Id).GetHashCode();


        public bool Equals(AgencyInfo other)
            => (Name, Id) == (other.Name, other.Id);


        public override bool Equals(object obj)
            => obj is AgencyInfo other && Equals(other);
    }
}