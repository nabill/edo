namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusPayByLinkRequest
    {
        public string EmailAddress { get; init; }
        public NGeniusBillingAddress BillingAddress { get; init; }
    }
}