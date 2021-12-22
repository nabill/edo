using HappyTravel.Edo.Notifications.Enums;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public class RecipientWithSendingSettings
    {
        public int RecipientId { get; init; }
        public Dictionary<ProtocolTypes, object> SendingSettings { get; init; }
    }
}
