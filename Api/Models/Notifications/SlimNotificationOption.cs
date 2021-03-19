using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Notifications;

namespace HappyTravel.Edo.Api.Models.Notifications
{
    public readonly struct SlimNotificationOption
    {
        public ProtocolTypes EnabledProtocols { get; init; }
        public bool IsMandatory { get; init; }
    }
}