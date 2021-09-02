namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusPayLinkRequest
    {
        public Payment Card { get; init; }
        public string EmailAddress { get; init; }
        public NGeniusBillingAddress BillingAddress { get; init; }
    }
}