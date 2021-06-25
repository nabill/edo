using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Notifications.Models
{
    public readonly struct SlimNotificationOptions
    {
        public SlimNotificationOptions(ProtocolTypes enabledProtocols, bool isMandatory, ReceiverTypes enabledReceivers)
        {
            EnabledProtocols = enabledProtocols;
            IsMandatory = isMandatory;
            EnabledReceivers = enabledReceivers;
        }


        public ProtocolTypes EnabledProtocols { get; init; }
        public bool IsMandatory { get; init; }
        public ReceiverTypes EnabledReceivers { get; init; }
    }
}