using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct SlimAgencyInfo
    {
        [JsonConstructor]
        public SlimAgencyInfo(string name, string address, string billingEmail, string city,
            string countryCode, string countryName, string fax, string phone, string postalCode, string website, 
            string vatNumber, PaymentTypes defaultPaymentType, List<int> ancestors, string countryHtId, string localityHtId,
            AgencyVerificationStates verificationState, DateTime? verificationDate, string legalAddress, PaymentTypes preferredPaymentMethod,
            bool isContractUploaded)
        {
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
            DefaultPaymentType = defaultPaymentType;
            Ancestors = ancestors;
            CountryHtId = countryHtId;
            LocalityHtId = localityHtId;
            VerificationState = verificationState;
            VerificationDate = verificationDate;
            LegalAddress = legalAddress;
            PreferredPaymentMethod = preferredPaymentMethod;
            IsContractUploaded = isContractUploaded;
        }


        /// <summary>
        ///     Name of the agency.
        /// </summary>
        [Required]
        public string Name { get; }

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
    }
}