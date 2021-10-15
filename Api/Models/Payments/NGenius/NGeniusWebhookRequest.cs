namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusWebhookRequest
    {
        public string EventName { get; init; }
        public NGeniusOrder Order { get; init; }
    }
}