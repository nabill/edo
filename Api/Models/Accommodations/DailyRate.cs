using System;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct DailyRate
    {
        public DailyRate(DateTime fromDate, in DateTime toDate, in MoneyAmount finalPrice, in MoneyAmount gross, PriceTypes type,
            string description)
        {
            Description = description ?? string.Empty;
            FromDate = fromDate;
            Gross = gross;
            FinalPrice = finalPrice;
            ToDate = toDate;
            Type = type;
        }


        public static DailyRate FromDailyRate(EdoContracts.General.DailyRate dailyRate)
        {
            return new DailyRate(dailyRate.FromDate, 
                dailyRate.ToDate, 
                dailyRate.FinalPrice,
                dailyRate.Gross,
                dailyRate.Type,
                dailyRate.Description);
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
        public Currencies Currency => FinalPrice.Currency;

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
        public MoneyAmount FinalPrice { get; }

        /// <summary>
        ///     The price type.
        /// </summary>
        public PriceTypes Type { get; }
    }
}