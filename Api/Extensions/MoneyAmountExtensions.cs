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


        public static MoneyAmount ApplyCommission(this MoneyAmount moneyAmount, decimal commission)
            => new()
            {
                Amount = moneyAmount.Amount * (100 + commission) / 100,
                Currency = moneyAmount.Currency
            };
    }
}