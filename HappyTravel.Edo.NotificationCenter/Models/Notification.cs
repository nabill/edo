using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Notifications;
using HappyTravel.Edo.Common.Models.Notifications;

namespace HappyTravel.Edo.NotificationCenter.Models
{
    public readonly struct Notification
    {
        public int UserId { get; init; }
        public string Message { get; init; }
        public Dictionary<ProtocolTypes, ISendingSettings> SendingSettings { get; init; }
    }
}