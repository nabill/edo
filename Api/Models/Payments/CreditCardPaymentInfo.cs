using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct CreditCardPaymentInfo
    {
        [JsonConstructor]
        public CreditCardPaymentInfo(string customerIp, string externalId, string message, string authorizationCode, string expirationDate)
        {
            CustomerIp = customerIp;
            ExternalId = externalId;
            Message = message;
            AuthorizationCode = authorizationCode;
            ExpirationDate = expirationDate;
        }


        public string CustomerIp { get; }
        public string ExternalId { get; }
        public string Message { get; }
        public string AuthorizationCode { get; }
        public string ExpirationDate { get; }
    }
}
