using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct GetOneTimeTokenRequest
    {
        [JsonConstructor]
        public GetOneTimeTokenRequest(string number, string securityCode, string expiryDate, string holderName)
        {
            Number = number;
            ExpirationDate = expiryDate;
            HolderName = holderName;
            SecurityCode = securityCode;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public string SecurityCode { get; }
    }
}
