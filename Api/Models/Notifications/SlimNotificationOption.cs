using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.Models.Notifications
{
    public readonly struct SlimNotificationOptions
    {
        public ProtocolTypes EnabledProtocols { get; init; }
        public bool IsMandatory { get; init; }
    }
}