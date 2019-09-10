namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public class PayfortPaymentRequest
    {
        public string Command { get; set; }
        public string AccessCode { get; set; }
        public string MerchantIdentifier{ get; set; }
        public string MerchantReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerIp { get; set; }
        public string TokenName { get; set; }
        public string Signature { get; set; }
        public string PaymentOption { get; set; }
        public string Eci { get; set; }
        public string OrderDescription { get; set; }
        public string CardSecurityCode { get; set; }
        public string CustomerName { get; set; }
        public string MerchantExtra { get; set; }
        public string MerchantExtra1 { get; set; }
        public string MerchantExtra2 { get; set; }
        public string MerchantExtra4 { get; set; }
        public string MerchantExtra5 { get; set; }
        public string RememberMe { get; set; }
        public string PhoneNumber { get; set; }
        public string SettlementReference { get; set; }
        public string ReturnUrl { get; set; }
    }
}
