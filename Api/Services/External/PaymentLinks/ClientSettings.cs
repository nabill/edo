using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.External.PaymentLinks
{
    public class ClientSettings
    {
        /// <summary>
        ///     Available currencies.
        /// </summary>
        public List<Currencies> Currencies { get; set; }

        /// <summary>
        ///     Available service types
        /// </summary>
        public Dictionary<ServiceTypes, string> ServiceTypes { get; set; }
    }
}