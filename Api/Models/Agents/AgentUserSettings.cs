using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentUserSettings
    {
        [JsonConstructor]
        public AgentUserSettings(bool isEndClientMarkupsEnabled, Currencies paymentsCurrency, Currencies displayCurrency, int bookingReportDays)
        {
            IsEndClientMarkupsEnabled = isEndClientMarkupsEnabled;
            PaymentsCurrency = paymentsCurrency;
            DisplayCurrency = displayCurrency;
            BookingReportDays = bookingReportDays;
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
        ///     Currency to show availability results in.
        /// </summary>
        public Currencies DisplayCurrency { get; }

        /// <summary>
        ///     How many days from current date should be taken into booking summary report. Should be in range from 1 to 7, with default 3.
        /// </summary>
        public int BookingReportDays { get; }
    }
}