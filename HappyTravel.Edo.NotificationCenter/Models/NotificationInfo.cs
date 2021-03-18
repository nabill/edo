using HappyTravel.Edo.NotificationCenter.Enums;

namespace HappyTravel.Edo.NotificationCenter.Models
{
    public readonly struct NotificationInfo
    {
        public int AgentId { get; init; }
        public string Message { get; init; }
        public ProtocolTypes[] Protocols { get; init; }
        public EmailSettings? EmailSettings { get; init; }
    }


    public readonly struct EmailSettings
    {
        public string Email { get; init; }
        public string TemplateId { get; init; }
        public object Data { get; init; }
    }
}