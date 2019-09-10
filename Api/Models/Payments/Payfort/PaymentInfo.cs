namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct PaymentInfo
    {
        public PaymentInfo(string secure3d, string referenceCode, string authorizationCode, string externalCode, string expiryDate, string cardNumber, string cardHolderName)
        {
            Secure3d = secure3d;
            ReferenceCode = referenceCode;
            AuthorizationCode = authorizationCode;
            ExternalCode = externalCode;
            ExpiryDate = expiryDate;
            CardNumber = cardNumber;
            CardHolderName = cardHolderName;
        }

        public string Secure3d { get; }
        public string ReferenceCode { get; }
        public string AuthorizationCode { get; }
        public string ExternalCode { get; }
        public string ExpiryDate { get; }
        public string CardNumber { get; }
        public string CardHolderName { get; }
    }
}
