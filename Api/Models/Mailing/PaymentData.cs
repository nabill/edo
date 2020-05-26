namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class PaymentData : DataWithCompanyInfo
    {
        public string Amount { get; set; }
        public string CustomerName { get; set; }
        public string Date { get; set; }
        public string Method { get; set; }
        public string ReferenceCode { get; set; }
    }
}