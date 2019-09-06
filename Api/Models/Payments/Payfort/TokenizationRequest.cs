namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct TokenizationRequest
    {
        public TokenizationRequest(string cardNumber, string cardHolderName, string cardSecurityCode, string expiryDate, bool rememberMe)
        {
            CardNumber = cardNumber;
            CardHolderName = cardHolderName;
            CardSecurityCode = cardSecurityCode;
            ExpiryDate = expiryDate;
            RememberMe = rememberMe;
        }

        public string CardNumber { get; }
        public string CardHolderName { get; }
        public string CardSecurityCode { get; }
        public string ExpiryDate { get; }
        public bool RememberMe { get; }
    }
}
