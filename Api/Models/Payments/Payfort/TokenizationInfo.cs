namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct TokenizationInfo
    {
        public TokenizationInfo(string expiryDate, string cardNumber, string tokenName, string cardHolderName)
        {
            ExpiryDate = expiryDate;
            CardNumber = cardNumber;
            TokenName = tokenName;
            CardHolderName = cardHolderName;
        }

        public string ExpiryDate { get; }
        public string CardNumber { get; }
        public string TokenName { get; }
        public string CardHolderName { get; }
    }
}
