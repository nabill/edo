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
                    { (int)NotificationTypes.BookingVoucher, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.BookingInvoice, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp) },
                    { (int)NotificationTypes.DeadlineApproaching, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.SuccessfulPaymentReceipt, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp) },
                    { (int)NotificationTypes.BookingDuePaymentDate, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.None },
                    { (int)NotificationTypes.BookingCancelled, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp) },
                    { (int)NotificationTypes.BookingFinalized, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp) },
                    { (int)NotificationTypes.BookingStatusChanged, (int)ProtocolTypes.WebSocket, false, (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp) },
                    { (int)NotificationTypes.CreditCardPaymentReceived, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.CounterpartyAccountBalanceReplenished, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.AgentInvitation, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.ChildAgencyInvitation, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.AgentSuccessfulRegistration, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.ChildAgencySuccessfulRegistration, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.AgencyActivityChanged, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.AdministratorInvitation, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AdminPanel },
                    { (int)NotificationTypes.MasterAgentSuccessfulRegistration, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { (int)NotificationTypes.BookingsAdministratorSummaryNotification, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { (int)NotificationTypes.BookingAdministratorPaymentsSummary, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel },    // TODO: Need clarify
                    { (int)NotificationTypes.BookingCancelledToReservations, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel }, // TODO: The WebSocket protocol will be added in the task AA-257
                    { (int)NotificationTypes.BookingFinalizedToReservations, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
                    { (int)NotificationTypes.CreditCardPaymentReceivedAdministrator, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel },   // TODO: The WebSocket protocol will be added in the task AA-257
                    { (int)NotificationTypes.BookingManualCorrectionNeeded, (int)ProtocolTypes.Email, false, (int)ReceiverTypes.AdminPanel }, // TODO: Need clarify
                    { (int)NotificationTypes.BookingSummaryReportForAgent, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AgentApp }, // TODO: Need clarify
                    { (int)NotificationTypes.ExternalPaymentLinks, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.PaymentLinkPaidNotification, (int)ProtocolTypes.Email, true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.CounterpartyActivityChanged, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.AgencyVerificationChanged, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), true, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.CounterpartyAccountBalanceSubtracted, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.CounterpartyAccountBalanceIncreasedManually, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp },
                    { (int)NotificationTypes.CounterpartyAccountBalanceDecreasedManually, (int)(ProtocolTypes.Email | ProtocolTypes.WebSocket), false, (int)ReceiverTypes.AgentApp }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultNotificationOptions");
        }
    }
}
