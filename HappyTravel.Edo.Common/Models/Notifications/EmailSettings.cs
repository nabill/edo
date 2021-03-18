namespace HappyTravel.Edo.Common.Models.Notifications
{
    public readonly struct EmailSettings
    {
        public string Email { get; init; }
        public string TemplateId { get; init; }
        public object Data { get; init; }
    }
}