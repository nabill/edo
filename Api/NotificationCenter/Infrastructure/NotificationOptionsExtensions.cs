using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Notifications;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System;

namespace HappyTravel.Edo.Api.NotificationCenter.Infrastructure
{
    public static class NotificationOptionsExtensions
    {
        public static SlimNotificationOptions ToSlimNotificationOptions(this NotificationOptions notificationOptions)
        {
            return new SlimNotificationOptions(enabledProtocols: notificationOptions.EnabledProtocols,
                isMandatory: notificationOptions.IsMandatory,
                enabledReceivers: notificationOptions.UserType.ToReceiverType(),
                emailTemplateId: null);
        }


        public static ReceiverTypes ToReceiverType(this ApiCallerTypes apiCallerType)
            => apiCallerType switch
            {
                ApiCallerTypes.Admin => ReceiverTypes.AdminPanel,
                ApiCallerTypes.Agent => ReceiverTypes.AgentApp,
                ApiCallerTypes.PropertyOwner => ReceiverTypes.PropertyOwner,
                _ => throw new NotImplementedException("There is no corresponding receiver type for the specified API caller type")
            };
    }
}
