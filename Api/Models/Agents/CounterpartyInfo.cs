using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(int id, string name, string address, string countryCode, string countryName, string city, string phone,
            string fax, string postalCode, Currencies preferredCurrency, PaymentMethods preferredPaymentMethod, string website,
            string vatNumber, string billingEmail, bool isContractUploaded)
        {
            Id = id;
            Name = name;
            Address = address;
            CountryCode = countryCode;
            City = city;
            Phone = phone;
            Fax = fax;
            PostalCode = postalCode;
            PreferredCurrency = preferredCurrency;
            PreferredPaymentMethod = preferredPaymentMethod;
            Website = website;
            VatNumber = vatNumber;
            BillingEmail = billingEmail;
            IsContractUploaded = isContractUploaded;
            CountryName = countryName;
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
        public string City { get; }

        /// <summary>
        ///     Phone number. Only digits, length between 3 and 30.
        /// </summary>
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
        ///     Preferable payments currency.
        /// </summary>
        public Currencies PreferredCurrency { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        public PaymentMethods PreferredPaymentMethod { get; }

        /// <summary>
        ///     Counterparty site url.
        /// </summary>
        public string Website { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string VatNumber { get; }

        /// <summary>
        /// E-mail for billing operations
        /// </summary>
        public string BillingEmail { get; }

        /// <summary>
        /// True if contract is loaded to counterparty
        /// </summary>
        public bool IsContractUploaded { get; }
    }
}