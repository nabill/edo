using HappyTravel.EdoContracts.General.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerUserSettings
    {
        [JsonConstructor]
        public CustomerUserSettings(bool isEndClientMarkupsEnabled, Currencies paymentsCurrency, Currencies displayCurrency)
        {
            IsEndClientMarkupsEnabled = isEndClientMarkupsEnabled;
            PaymentsCurrency = paymentsCurrency;
            DisplayCurrency = displayCurrency;
        }


        /// <summary>
        ///     Apply end-client markups to search results and booking.
        /// </summary>
        public bool IsEndClientMarkupsEnabled { get; }

        /// <summary>
        ///     Currency of customer payments.
        /// </summary>
        public Currencies PaymentsCurrency { get; }
        
        
        /// <summary>
        /// Currency to show availability results in.
        /// </summary>
        public Currencies DisplayCurrency { get; }
    }
}