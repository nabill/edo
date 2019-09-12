namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct TokenizationInfo
    {
        public TokenizationInfo(string expiryDate, string cardNumber, string tokenName, string cardHolderName)
        {
            ExpirationDate = expiryDate;
            CardNumber = cardNumber;
            TokenName = tokenName;
            CardHolderName = cardHolderName;
        }

        public string ExpirationDate { get; }
        public string CardNumber { get; }
        public string TokenName { get; }
        public string CardHolderName { get; }
    }
}
