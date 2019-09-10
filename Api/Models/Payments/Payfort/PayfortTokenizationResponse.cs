namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public class PayfortTokenizationResponse
    {
        public string ServiceCommand { get; set; }
        public string AccessCode { get; set; }
        public string MerchantIdentifier{ get; set; }
        public string MerchantReference { get; set; }
        public string Language { get; set; }
        public string ExpiryDate { get; set; }
        public string CardNnumber { get; set; }
        public string Signature { get; set; }
        public string TokenName { get; set; }
        public string ResponseMessage { get; set; }
        public string ResponseCode { get; set; }
        public string Status { get; set; }
        public string CardBin { get; set; }
        public string CardHolderName { get; set; }
        public string RememberMe { get; set; }
        public string ReturnUrl { get; set; }
    }
}
