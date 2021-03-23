using System;

namespace HappyTravel.Edo.Common.Enums.Notifications
{
    [Flags]
    public enum ProtocolTypes
    {
        WebSocket = 1,
        Email = 2
    }
}