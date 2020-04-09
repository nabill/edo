namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class PaymentNotificationOptions
    {
        public string UnknownAgentTemplateId { get; set; }
        public string KnownAgentTemplateId { get; set; }
        public string NeedPaymentTemplateId { get; set; }
    }
}