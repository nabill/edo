namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class CounterpartyBillingNotificationServiceOptions
    {
        public string CounterpartyAccountAddedTemplateId { get; set; }
        public string CounterpartyAccountSubtractedTemplateId { get; set; }
        public string CounterpartyAccountIncreasedManuallyTemplateId { get; set; }
        public string CounterpartyAccountDecreasedManuallyTemplateId { get; set; }
    }
}
