using System;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct DailyRate
    {
        [JsonConstructor]
        public DailyRate(DateTimeOffset fromDate, in DateTimeOffset toDate, in MoneyAmount totalPrice, in MoneyAmount gross, PriceTypes type, string description)
        {
            Description = description;
            FromDate = fromDate;
            Gross = gross;
            TotalPrice = totalPrice;
            ToDate = toDate;
            Type = type;
        }


        /// <summary>
        ///     Start of the date range
        /// </summary>
        public DateTimeOffset FromDate { get; }

        /// <summary>
        ///     End of the date range
        /// </summary>
        public DateTimeOffset ToDate { get; }

        /// <summary>
        ///     Currency of the price
        /// </summary>
        public Currencies Currency => TotalPrice.Currency;

        /// <summary>
        ///     Description of the price
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gross price of a service (This is just a <b>reference</b> value)
        /// </summary>
        public MoneyAmount Gross { get; }

        /// <summary>
        ///     Final and total net price of a service (This is the <b>actual</b> value for the price)
        /// </summary>
        public MoneyAmount TotalPrice { get; }

        /// <summary>
        ///     Type of price
        /// </summary>
        public PriceTypes Type { get; }
    }
}