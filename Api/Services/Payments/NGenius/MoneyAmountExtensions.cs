using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.NGenius
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