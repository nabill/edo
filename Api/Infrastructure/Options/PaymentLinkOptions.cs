using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class PaymentLinkOptions
    {
        public ClientSettings ClientSettings { get; set; }
        public string PaymentUrlPrefix { get; set; }
    }
}