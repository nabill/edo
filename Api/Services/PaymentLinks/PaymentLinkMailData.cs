using HappyTravel.Edo.Api.Models.Payments.External;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkMailData
    {
        public PaymentLinkMailData(string code, PaymentLinkData linkData)
        {
            Code = code;
            LinkData = linkData;
        }
        
        public string Code { get; }
        public PaymentLinkData LinkData { get; }
    }
}