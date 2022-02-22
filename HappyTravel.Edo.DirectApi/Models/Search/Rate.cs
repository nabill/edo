using System.Collections.Generic;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct Rate
    {
        [JsonConstructor]
        public Rate(in MoneyAmount finalPrice, in MoneyAmount gross, List<Discount>? discounts = null, string? description = null)
        {
            Description = description;
            Gross = gross;
            Discounts = discounts ?? new List<Discount>();
            FinalPrice = finalPrice;
            Currency = finalPrice.Currency;
        }
        
        /// <summary>
        ///     Currency of the price
        /// </summary>
        public Currencies Currency { get; }

        /// <summary>
        ///     Description of the price
        /// </summary>
        public string? Description { get; }

        /// <summary>
        ///     Gross price of a service (This is just a <b>reference</b> value)
        /// </summary>
        public MoneyAmount Gross { get; }

        /// <summary>
        ///     List of available discounts
        /// </summary>
        public List<Discount> Discounts { get; }

        /// <summary>
        ///     Final and total net price of a service (This is the <b>actual</b> value for the price)
        /// </summary>
        public MoneyAmount FinalPrice { get; }
    }
}