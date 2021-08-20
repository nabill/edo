namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct OrderRequest
    {
        public Order Order { get; init; }
        public BillingAddress BillingAddress { get; init; }
        public Payment? Payment { get; init; }
        public SavedCard? SavedCard { get; init; } 
        public string MerchantOrderReference { get; init; }
    }
}