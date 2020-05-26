namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class PaymentDataWithLink : DataWithCompanyInfo
    {
       public string Amount { get; set; }
       public string Comment  { get; set; }
       public string PaymentLink  { get; set; }
       public string ServiceDescription  { get; set; }
    }
}