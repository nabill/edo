namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct ResponsePaymentInformation
    {
        public ResponsePaymentInformation(string pan, string expiry, string cvv, string cardholderName, string name)
        {
            Pan = pan;
            Expiry = expiry;
            Cvv = cvv;
            CardholderName = cardholderName;
            Name = name;
        }


        public string Pan { get; }
        public string Expiry { get; }
        public string Cvv { get; }
        public string CardholderName { get; }
        public string Name { get; }
    }
}