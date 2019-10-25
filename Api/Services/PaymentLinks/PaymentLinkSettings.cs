using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkSettings
    {
        /// <summary>
        ///     Available currencies.
        /// </summary>
        public List<string> Currencies { get; set; }

        /// <summary>
        ///     Available facilities.
        /// </summary>
        public List<string> Facilities { get; set; }

        /// <summary>
        ///     Default currency.
        /// </summary>
        public string DefaultCurrency { get; set; }

        /// <summary>
        ///     Default facility.
        /// </summary>
        public string DefaultFacility { get; set; }
    }
}