namespace HappyTravel.Edo.Notifications.Models
{
    public readonly struct EmailSettings : ISendingSettings
    {
        public string Email { get; init; }
        public string TemplateId { get; init; }
    }
}