namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public struct SavedCard
    {
        public string CardToken { get; init; }
        public string CardHolderName { get; init; }
        public string Expiry { get; init; }
        public string MaskedPan { get; init; }
        public string Scheme { get; init; }
        public string Cvv { get; init; }
        public bool RecaptureCsc { get; init; }
    }
}