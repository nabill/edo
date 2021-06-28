using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct EditAgencyRequest
    {
        [JsonConstructor]
        public EditAgencyRequest(string address, string billingEmail, string fax, string phone, string postalCode, string website, string vatNumber)
        {
            Address = address;
            BillingEmail = billingEmail;
            Fax = fax;
            Phone = phone;
            PostalCode = postalCode;
            Website = website;
            VatNumber = vatNumber;
        }

        /// <summary>
        ///     Agency address.
        /// </summary>
        public string Address { get; }

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
        ///     Agency site url.
        /// </summary>
        public string Website { get; }

        /// <summary>
        /// E-mail for billing operations
        /// </summary>
        public string BillingEmail { get; }

        /// <summary>
        ///     Vat number of an agency
        /// </summary>
        public string VatNumber { get; }
    }
}