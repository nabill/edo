using HappyTravel.Edo.Notifications.Enums;
using System.Collections.Generic;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public class NotificationSettings
    {
        public Dictionary<ProtocolTypes, bool> EnabledProtocols { get; init; }
        public bool IsMandatory { get; init; }
    }
}
