using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentUserSettings
    {
        [JsonConstructor]
        public AgentUserSettings(bool isEndClientMarkupsEnabled, Currencies paymentsCurrency, Currencies displayCurrency)
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
        ///     Currency of agent payments.
        /// </summary>
        public Currencies PaymentsCurrency { get; }
        
        
        /// <summary>
        /// Currency to show availability results in.
        /// </summary>
        public Currencies DisplayCurrency { get; }
    }
}