using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusAmount
    {
        public Currencies CurrencyCode { get; init; }
        public int Value { get; init; }
    }
}