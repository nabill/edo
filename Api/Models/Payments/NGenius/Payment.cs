namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public struct Payment
    {
        public string Pan { get; init; }
        public string Expiry { get; init; }
        public string Cvv { get; init; }
        public string CardholderName { get; init; }
    }
}