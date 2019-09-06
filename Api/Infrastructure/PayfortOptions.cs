namespace HappyTravel.Edo.Api.Infrastructure
{
    public class PayfortOptions
    {
        public string Identifier { get; set; }
        public string AccessCode { get; set; }
        public string Reference { get; set; }
        public string TokenizationUrl { get; set; }
        public string PaymentUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string SHARequestPhrase { get; set; }
        public string SHAResponsePhrase { get; set; }
    }
}
