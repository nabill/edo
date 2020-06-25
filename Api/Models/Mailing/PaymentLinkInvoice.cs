using System;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class PaymentLinkInvoice : DataWithCompanyInfo
    {
       public string Amount { get; set; }
       public string Comment  { get; set; }
       public string PaymentLink  { get; set; }
       public string ServiceDescription  { get; set; }
       public string Number { get; set; }
       public DateTime Date { get; set; }
    }
}