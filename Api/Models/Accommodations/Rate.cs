using System.Collections.Generic;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct Rate
    {
        [JsonConstructor]
        public Rate(in MoneyAmount finalPrice, in MoneyAmount gross, List<Discount>? discounts = null,
            PriceTypes type = PriceTypes.Room, string? description = null, decimal commission = 0m, MoneyAmount netPrice = default)
        {
            Description = description ?? string.Empty;
            Gross = gross;
            Discounts = discounts ?? new List<Discount>();
            FinalPrice = finalPrice;
            Type = type;
            Currency = finalPrice.Currency;
            Commission = commission;
            NetPrice = (netPrice != default && commission != 0m) ? netPrice : finalPrice;
        }

        /// <summary>
        ///     The price currency.
        /// </summary>
        public Currencies Currency { get; }

        /// <summary>
        ///     The price description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        ///     The gross price of a service. This is just <b>a reference</b> value.
        /// </summary>
        public MoneyAmount Gross { get; }

        /// <summary>
        ///     The list of available discounts.
        /// </summary>
        public List<Discount>? Discounts { get; }

        /// <summary>
        ///     Commission according to payment type or agency's contract kind
        /// </summary>
        public decimal Commission { get; }

        /// <summary>
        ///     Net price of a service before applying commission
        /// </summary>
        public MoneyAmount NetPrice { get; }

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