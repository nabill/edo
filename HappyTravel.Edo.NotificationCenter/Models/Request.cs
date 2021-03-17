using HappyTravel.Edo.NotificationCenter.Enums;

namespace HappyTravel.Edo.NotificationCenter.Models
{
    public readonly struct Request
    {
        public int AgentId { get; init; }
        public string? Email { get; init; }
        public MessageType MessageType { get; init; }
        public string ShortMessage { get; init; }
        public string Message { get; init; }
        public ProtocolTypes[] Protocols { get; init; }
    }
}