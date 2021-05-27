using CSharpFunctionalExtensions;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System.Collections.Generic;

namespace HappyTravel.Edo.Notifications.Infrastructure
{
    public static class NotificationOptionsHelper
    {
        public static Result<SlimNotificationOptions> TryGetDefaultOptions(NotificationTypes type) 
            => _defaultOptions.TryGetValue(type, out var value)
                ? value
                : Result.Failure<SlimNotificationOptions>($"Cannot find options for type '{type}'");


        private static readonly Dictionary<NotificationTypes, SlimNotificationOptions> _defaultOptions = new()
        {
            // Booking
            [NotificationTypes.BookingVoucher] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true },
            [NotificationTypes.BookingInvoice] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true },
            [NotificationTypes.DeadlineApproaching] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.SuccessfulPaymentReceipt] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true },
            [NotificationTypes.BookingDuePaymentDate] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.BookingCancelled] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.BookingFinalized] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.BookingStatusChanged] = new() { EnabledProtocols = ProtocolTypes.WebSocket, IsMandatory = false },
            // Accounts
            [NotificationTypes.CreditCardPaymentReceived] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.AccountBalanceReplenished] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            // Counterparty
            [NotificationTypes.RegularCustomerInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true },
            [NotificationTypes.ChildAgencyInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true },
            [NotificationTypes.RegularCustomerSuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.ChildAgencySuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.AgencyManagement] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            // Administrator
            [NotificationTypes.AdministratorInvitation] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true },
            [NotificationTypes.MasterCustomerSuccessfulRegistration] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true },  // TODO: The WebSocket protocol will be added in the task AA-257
            [NotificationTypes.BookingsAdministratorSummaryNotification] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.BookingCancelledToReservations] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.BookingFinalizedToReservations] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            [NotificationTypes.CreditCardPaymentReceivedAdministrator] = new() { EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false },
            // Other
            [NotificationTypes.BookingSummaryReportForAgent] = new() { EnabledProtocols = ProtocolTypes.Email, IsMandatory = true }
        };
    }
}
