using HappyTravel.Edo.Notifications.Enums;
using System.Collections.Generic;

namespace HappyTravel.Edo.Notifications.Models
{
    public readonly struct Notification
    {
        public int UserId { get; init; }
        public ReceiverTypes Receiver { get; init; }
        public string Message { get; init; }
        public NotificationTypes Type { get; init; }
        public Dictionary<ProtocolTypes, ISendingSettings> SendingSettings { get; init; }
    }
}