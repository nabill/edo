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
        public DailyRate(DateTime fromDate, in DateTime toDate, in MoneyAmount finalPrice, in MoneyAmount gross, PriceTypes type,
            string description)
        {
            // TODO: check nullability
            Description = description ?? string.Empty;
            FromDate = fromDate;
            Gross = gross;
            FinalPrice = finalPrice;
            ToDate = toDate;
            Type = type;
        }


        /// <summary>
        ///     Start of the date range
        /// </summary>
        public DateTime FromDate { get; }

        /// <summary>
        ///     End of the date range
        /// </summary>
        public DateTime ToDate { get; }

        // TODO: what's the difference between final and total prices?
        /// <summary>
        ///     Currency of the price
        /// </summary>
        public Currencies Currency => FinalPrice.Currency;

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
        public MoneyAmount FinalPrice { get; }

        /// <summary>
        ///     Type of price
        /// </summary>
        public PriceTypes Type { get; }
    }
}