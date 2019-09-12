using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public class PayfortPaymentResponse
    {
        public string Command { get; set; }
        public string AccessCode { get; set; }
        public string MerchantIdentifier { get; set; }
        public string MerchantReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerIp { get; set; }
        public string TokenName { get; set; }
        public string Signature { get; set; }
        public string FortId { get; set; }
        public string PaymentOption { get; set; }
        public string Eci { get; set; }
        public string OrderDescription { get; set; }
        public string AuthorizationCode { get; set; }
        public string ResponseMessage { get; set; }
        public string ResponseCode { get; set; }
        public string CustomerName { get; set; }
        public string MerchantExtra { get; set; }
        public string MerchantExtra1 { get; set; }
        public string MerchantExtra2 { get; set; }
        public string MerchantExtra4 { get; set; }
        public string MerchantExtra5 { get; set; }
        [JsonProperty("expiry_date")]
        public string ExpirationDate { get; set; }
        public string CardNumber { get; set; }
        public string Status { get; set; }
        public string CardHolderName { get; set; }
        [JsonProperty("3ds_url")]
        public string Secure3D {get; set;}
        public string RememberMe { get; set; }
        public string PhoneNumber { get; set; }
        public string SettlementReference { get; set; }
    }
}
