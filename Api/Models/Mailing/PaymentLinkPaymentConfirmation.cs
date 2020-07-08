namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class PaymentLinkPaymentConfirmation : DataWithCompanyInfo
    {
        public string Date { get; set; }
        public string Amount { get; set; }
        public string ReferenceCode { get; set; }
        public string ServiceDescription { get; set; }
    }
}