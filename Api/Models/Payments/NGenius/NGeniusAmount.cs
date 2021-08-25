namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusAmount
    {
        public string CurrencyCode { get; init; }
        public int Value { get; init; }
    }
}