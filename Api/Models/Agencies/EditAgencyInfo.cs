using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agencies
{
    public readonly struct EditAgencyInfo
    {
        [JsonConstructor]
        public EditAgencyInfo(string address, string billingEmail, string fax, string phone, string postalCode, string website)
        {
            Address = address;
            BillingEmail = billingEmail;
            Fax = fax;
            Phone = phone;
            PostalCode = postalCode;
            Website = website;
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
    }
}