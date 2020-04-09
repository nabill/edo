using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct CreditCardPaymentInfo
    {
        [JsonConstructor]
        public CreditCardPaymentInfo(string agentIp, string externalId, string message, string authorizationCode, string expirationDate, string internalReferenceCode)
        {
            AgentIp = agentIp;
            ExternalId = externalId;
            Message = message;
            AuthorizationCode = authorizationCode;
            ExpirationDate = expirationDate;
            InternalReferenceCode = internalReferenceCode;
        }


        public string AgentIp { get; }
        public string ExternalId { get; }
        public string Message { get; }
        public string AuthorizationCode { get; }
        public string ExpirationDate { get; }
        public string InternalReferenceCode { get; }
    }
}