using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public class PayfortAuthorizeResponse : IPayfortResponse
    {
        [JsonConstructor]
        public PayfortAuthorizeResponse(string signature, string fortId, string authorizationCode, string responseMessage, string responseCode,
            string expiryDate, string cardNumber, string status, [JsonProperty("3ds_url")] string secure3d, string settlementReference)
        {
            Signature = signature;
            FortId = fortId;
            AuthorizationCode = authorizationCode;
            ResponseMessage = responseMessage;
            ResponseCode = responseCode;
            ExpirationDate = expiryDate;
            CardNumber = cardNumber;
            Status = status;
            Secure3d = secure3d;
            SettlementReference = settlementReference;
        }

 
        public string Signature { get; }
        public string FortId { get; }
        public string AuthorizationCode { get; }
        public string ResponseMessage { get; }
        public string ResponseCode { get; }

        [JsonProperty("expiry_date")]
        public string ExpirationDate { get; }

        public string CardNumber { get; }
        public string Status { get; }

        [JsonProperty("3ds_url")]
        public string Secure3d { get; }

        public string SettlementReference { get; }
    }
}
