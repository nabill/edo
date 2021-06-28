﻿using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Data.Notifications
{
    public class DefaultNotificationOptions
    {
        public NotificationTypes Type { get; set; }
        public ProtocolTypes EnabledProtocols { get; set; }
        public bool IsMandatory { get; set; }
        public ReceiverTypes EnabledReceivers { get; set; }
    }
}
