using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct RegistrationAgencyInfo
    {
        [JsonConstructor]
        public RegistrationAgencyInfo(string name, string address, string billingEmail, string? fax,
            string phone, string? postalCode, string? website, string? vatNumber, string legalAddress, PaymentTypes preferredPaymentMethod,
            string localityHtId)
        {
            Name = name;
            Address = address;
            BillingEmail = billingEmail;
            Fax = fax;
            Phone = phone;
            PostalCode = postalCode;
            Website = website;
            VatNumber = vatNumber;
            LegalAddress = legalAddress;
            PreferredPaymentMethod = preferredPaymentMethod;
            LocalityHtId = localityHtId;
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
        ///     Phone number. Only digits, length between 3 and 30.
        /// </summary>
        [Required]
        public string Phone { get; }

        /// <summary>
        ///     Fax number. Only digits, length between 3 and 30.
        /// </summary>
        public string? Fax { get; }

        /// <summary>
        ///     Postal code.
        /// </summary>
        public string? PostalCode { get; }

        /// <summary>
        ///     Agency site url.
        /// </summary>
        public string? Website { get; }

        /// <summary>
        /// E-mail for billing operations
        /// </summary>
        [Required]
        public string BillingEmail { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string? VatNumber { get; }

        /// <summary>
        ///     Agency address.
        /// </summary>
        [Required]
        public string LegalAddress { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        public PaymentTypes PreferredPaymentMethod { get; }

        [Required]
        public string LocalityHtId { get; }
    }
}