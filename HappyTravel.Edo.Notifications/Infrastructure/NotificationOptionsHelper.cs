using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System.Collections.Generic;
using System.Linq;

namespace HappyTravel.Edo.Notifications.Infrastructure
{
    public static class NotificationOptionsHelper
    {
        public static Result<SlimNotificationOptions> TryGetDefaultOptions(NotificationTypes type, ApiCallerTypes userType)
        {
            var receiver = GetReceiver(userType);
            var options = _defaultOptions.TryGetValue(type, out var value)
                ? value
                : Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}'");

            return options.Value.EnabledReceivers.HasFlag(receiver)
                ? options
                : Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}' and the receiver '{receiver}'");


            static ReceiverTypes GetReceiver(ApiCallerTypes userType)
                => userType switch
                {
                    ApiCallerTypes.Admin => ReceiverTypes.AdminPanel,
                    ApiCallerTypes.Agent => ReceiverTypes.AgentApp,
                    _ => throw new System.NotImplementedException()
                };
        }


        public static Dictionary<NotificationTypes, SlimNotificationOptions> GetDefaultOptions(ReceiverTypes receiver)
            => _defaultOptions.Where(o => o.Value.EnabledReceivers.HasFlag(receiver))
                .ToDictionary(p => p.Key, p => p.Value);


        private static readonly Dictionary<NotificationTypes, SlimNotificationOptions> _defaultOptions = new()
        {
            // Booking
            [NotificationTypes.BookingVoucher] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true,
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.BookingInvoice] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
            [NotificationTypes.DeadlineApproaching] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false,
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.SuccessfulPaymentReceipt] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true,
                EnabledReceivers = ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
            [NotificationTypes.BookingDuePaymentDate] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },    // TODO: Need clarify, now not used
            [NotificationTypes.BookingCancelled] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
            [NotificationTypes.BookingFinalized] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
            [NotificationTypes.BookingStatusChanged] = new() { EnabledProtocols = ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
            // Accounts
            [NotificationTypes.CreditCardPaymentReceived] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.AccountBalanceReplenished] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            // Counterparty
            [NotificationTypes.AgentInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.ChildAgencyInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.AgentSuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.ChildAgencySuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.AgencyManagement] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AgentApp },
            // Administrator
            [NotificationTypes.AdministratorInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AdminPanel },
            [NotificationTypes.MasterAgentSuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AdminPanel },  // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.BookingsAdministratorSummaryNotification] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.BookingAdministratorPaymentsSummary] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel },    // Need clarify
            [NotificationTypes.BookingCancelledToReservations] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.BookingFinalizedToReservations] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.CreditCardPaymentReceivedAdministrator] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.BookingManualCorrectionNeeded] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = false, 
                EnabledReceivers = ReceiverTypes.AdminPanel },  // Need clarify
            // Other
            [NotificationTypes.BookingSummaryReportForAgent] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AgentApp }, // Need clarify
            [NotificationTypes.ExternalPaymentLinks] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true,
                EnabledReceivers = ReceiverTypes.AgentApp },
            [NotificationTypes.PaymentLinkPaidNotification] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true, 
                EnabledReceivers = ReceiverTypes.AgentApp }
        };
    }
}
