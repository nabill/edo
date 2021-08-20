namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct Order
    {
        public string Action { get; init; }
        public string MerchantOrderReference { get; init; }
        public BillingAddress BillingAddress { get; init; }
        public NGeniusAmount Amount { get; init; }
        public string Email { get; init; }
    }
}