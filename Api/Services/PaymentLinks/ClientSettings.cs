using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class ClientSettings
    {
        /// <summary>
        ///     Available currencies.
        /// </summary>
        public List<Currencies> Currencies { get; set; }

        /// <summary>
        ///     Available facilities.
        /// </summary>
        public List<string> Facilities { get; set; }
    }
}