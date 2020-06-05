using System.ComponentModel.DataAnnotations;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CounterpartyInfo
    {
        [JsonConstructor]
        public CounterpartyInfo(string name, string address, string countryCode, string city, string phone,
            string fax, string postalCode, Currencies preferredCurrency, PaymentMethods preferredPaymentMethod, string website,
            string vatNumber)
        {
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
        }


        /// <summary>
        ///     Counterparty name.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        ///     Counterparty address.
        /// </summary>
        [Required]
        public string Address { get; }

        /// <summary>
        ///     Two-letter international country code.
        /// </summary>
        [Required]
        public string CountryCode { get; }

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
        ///     Preferable payments currency.
        /// </summary>
        [Required]
        public Currencies PreferredCurrency { get; }

        /// <summary>
        ///     Preferable way to do payments.
        /// </summary>
        [Required]
        public PaymentMethods PreferredPaymentMethod { get; }

        /// <summary>
        ///     Counterparty site url.
        /// </summary>
        public string Website { get; }

        /// <summary>
        /// Value added tax identification number
        /// </summary>
        public string VatNumber { get; }
    }
}