using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class MoneyAmountExtensions
    {
        public static NGeniusAmount ToNGeniusAmount(this MoneyAmount moneyAmount) 
            => new()
            {
                CurrencyCode = moneyAmount.Currency,
                Value = moneyAmount.ToFractionalUnits()
            };
    }
}