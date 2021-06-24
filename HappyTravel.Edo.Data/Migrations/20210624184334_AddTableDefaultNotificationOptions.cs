using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddTableDefaultNotificationOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefaultNotificationOptions",
                columns: table => new
                {
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EnabledProtocols = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    EnabledReceivers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultNotificationOptions", x => x.Type);
                });

            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "EnabledReceivers" }, 
                new object[,]
                {
                    { NotificationTypes.BookingVoucher, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.BookingInvoice, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
                    { NotificationTypes.DeadlineApproaching, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.SuccessfulPaymentReceipt, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
                    { NotificationTypes.BookingDuePaymentDate, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.None },
                    { NotificationTypes.BookingCancelled, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
                    { NotificationTypes.BookingFinalized, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
                    { NotificationTypes.BookingStatusChanged, ProtocolTypes.WebSocket, false, ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp },
                    { NotificationTypes.CreditCardPaymentReceived, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyAccountBalanceReplenished, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.AgentInvitation, ProtocolTypes.Email, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.ChildAgencyInvitation, ProtocolTypes.Email, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.AgentSuccessfulRegistration, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.ChildAgencySuccessfulRegistration, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.AgencyActivityChanged, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.AdministratorInvitation, ProtocolTypes.Email, true, ReceiverTypes.AdminPanel },
                    { NotificationTypes.MasterAgentSuccessfulRegistration, ProtocolTypes.Email, true, ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { NotificationTypes.BookingsAdministratorSummaryNotification, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { NotificationTypes.BookingAdministratorPaymentsSummary, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel },    // TODO: Need clarify
                    { NotificationTypes.BookingCancelledToReservations, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { NotificationTypes.BookingFinalizedToReservations, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
                    { NotificationTypes.CreditCardPaymentReceivedAdministrator, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
                    { NotificationTypes.BookingManualCorrectionNeeded, ProtocolTypes.Email, false, ReceiverTypes.AdminPanel }, // TODO: Need clarify
                    { NotificationTypes.BookingSummaryReportForAgent, ProtocolTypes.Email, true, ReceiverTypes.AgentApp }, // TODO: Need clarify
                    { NotificationTypes.ExternalPaymentLinks, ProtocolTypes.Email, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.PaymentLinkPaidNotification, ProtocolTypes.Email, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyActivityChanged, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyVerificationChanged, ProtocolTypes.Email | ProtocolTypes.WebSocket, true, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyAccountBalanceSubtracted, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyAccountBalanceIncreasedManually, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp },
                    { NotificationTypes.CounterpartyAccountBalanceDecreasedManually, ProtocolTypes.Email | ProtocolTypes.WebSocket, false, ReceiverTypes.AgentApp }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultNotificationOptions");
        }
    }
}
