using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct GetOneTimeTokenRequest
    {
        [JsonConstructor]
        public GetOneTimeTokenRequest(string number, string securityCode, string expirationDate, string holderName)
        {
            Number = number;
            ExpirationDate = expirationDate;
            HolderName = holderName;
            SecurityCode = securityCode;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public string SecurityCode { get; }
    }
}
