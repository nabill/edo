using System;

namespace HappyTravel.Edo.Notifications.Enums
{
    [Flags]
    public enum ReceiverTypes
    {
        None = 0,
        AdminPanel = 1,
        AgentApp = 2,
        PropertyOwner = 4
    }
}
