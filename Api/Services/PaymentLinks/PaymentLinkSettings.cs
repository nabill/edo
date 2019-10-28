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
    }
}