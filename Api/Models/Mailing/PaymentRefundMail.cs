namespace HappyTravel.Edo.Api.Models.Mailing;

public class PaymentRefundMail : DataWithCompanyInfo
{
    public string ReferenceCode { get; set; }
    public string RefundedAmount { get; set; }
}