namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public readonly struct MerchantAttributes
    {
        public string RedirectUrl { get; init; }
        public string CancelUrl { get; init; }
        public string CancelText { get; init; }
        
    }
}