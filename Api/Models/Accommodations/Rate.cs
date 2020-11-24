using System.Collections.Generic;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct Rate
    {
        public Rate(in MoneyAmount finalPrice, in MoneyAmount gross, List<Discount> discounts,
            PriceTypes type, string description)
        {
            Description = description;
            Gross = gross;
            Discounts = discounts;
            FinalPrice = finalPrice;
            Type = type;
            Currency = finalPrice.Currency;
        }
        
        public Currencies Currency { get; }

        public string Description { get; }

        public MoneyAmount Gross { get; }

        public List<Discount> Discounts { get; }

        public MoneyAmount FinalPrice { get; }

        public PriceTypes Type { get; }
    }
}