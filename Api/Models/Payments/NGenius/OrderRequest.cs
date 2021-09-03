namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct OrderRequest
    {
        public string Action { get; init; }
        public string MerchantOrderReference { get; init; }
        public NGeniusAmount Amount { get; init; }
        public string EmailAddress { get; init; }
        public NGeniusBillingAddress BillingAddress { get; init; }
    }
}