using HappyTravel.Edo.Api.Models.Payments.External;

namespace HappyTravel.Edo.Api.Services.PaymentLinks
{
    public class PaymentLinkMailData
    {
        public PaymentLinkMailData(string url, PaymentLinkData linkData, string serviceDescription)
        {
            Url = url;
            LinkData = linkData;
            ServiceDescription = serviceDescription;
        }
        
        public string Url { get; }
        public PaymentLinkData LinkData { get; }
        public string ServiceDescription { get; }
    }
}