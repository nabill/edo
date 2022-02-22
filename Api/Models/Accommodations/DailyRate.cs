using System;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct DailyRate
    {
        [JsonConstructor]
        public DailyRate(DateTime fromDate, in DateTime toDate, in MoneyAmount totalPrice, in MoneyAmount gross, PriceTypes type,
            string description)
        {
            Description = description ?? string.Empty;
            FromDate = fromDate;
            Gross = gross;
            TotalPrice = totalPrice;
            ToDate = toDate;
            Type = type;
        }


        /// <summary>
        ///     The time frame start date.
        /// </summary>
        public DateTime FromDate { get; }

        /// <summary>
        ///     The time frame end date.
        /// </summary>
        public DateTime ToDate { get; }

        /// <summary>
        ///     The price currency.
        /// </summary>
        public Currencies Currency => TotalPrice.Currency;

        /// <summary>
        ///     The price description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The gross price of a service. This is just <b>a reference</b> value.
        /// </summary>
        public MoneyAmount Gross { get; }

        /// <summary>
        ///     The final and total net price of a service. This is <b>the actual</b> value of a price.
        /// </summary>
        public MoneyAmount TotalPrice { get; }

        /// <summary>
        ///     The price type.
        /// </summary>
        public PriceTypes Type { get; }
    }
}