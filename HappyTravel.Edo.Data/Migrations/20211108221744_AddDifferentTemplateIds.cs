using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddDifferentTemplateIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailTemplateId",
                table: "DefaultNotificationOptions",
                newName: "AgentEmailTemplateId");

            migrationBuilder.AddColumn<string>(
                name: "AdminEmailTemplateId",
                table: "DefaultNotificationOptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyOwnerEmailTemplateId",
                table: "DefaultNotificationOptions",
                type: "text",
                nullable: true);

            var table = "DefaultNotificationOptions";
            var keyColumn = "Type";
            var column = "AdminEmailTemplateId";

            migrationBuilder.UpdateData(table, keyColumn, 2, "AdminEmailTemplateId", "d-9baff8dd811749e0afc1ffb80f0c9f1d"); // BookingInvoice
            migrationBuilder.UpdateData(table, keyColumn, 4, "AdminEmailTemplateId", "d-646b7c9a3b61442c9327b43ec77d9ac5"); // SuccessfulPaymentReceipt
            migrationBuilder.UpdateData(table, keyColumn, 6, "AdminEmailTemplateId", "d-378dbbbe87194aa1bb7dfa1338169739"); // BookingCancelled
            migrationBuilder.UpdateData(table, keyColumn, 7, "AdminEmailTemplateId", "d-9addc6e97b154ce9afdec1305e5eeddf"); // BookingFinalized

            migrationBuilder.UpdateData(table, keyColumn, 16, "AgentEmailTemplateId", null); // AdministratorInvitation
            migrationBuilder.UpdateData(table, keyColumn, 16, "AdminEmailTemplateId", "d-579c74eedd3a4624bd2bcd87535e49f0"); // AdministratorInvitation
            
            migrationBuilder.UpdateData(table, keyColumn, 17, "AgentEmailTemplateId", null); // MasterAgentSuccessfulRegistration
            migrationBuilder.UpdateData(table, keyColumn, 17, "AdminEmailTemplateId", "d-25ff2449ce9949e0aadb7bde35d6f2dc"); // MasterAgentSuccessfulRegistration

            migrationBuilder.UpdateData(table, keyColumn, 18, "AgentEmailTemplateId", null); // BookingsAdministratorSummaryNotification
            migrationBuilder.UpdateData(table, keyColumn, 18, "AdminEmailTemplateId", "d-0e0a051dfa62481aa54e3e3bc2ee7d0b"); // BookingsAdministratorSummaryNotification

            migrationBuilder.UpdateData(table, keyColumn, 19, "AgentEmailTemplateId", null); // BookingAdministratorPaymentsSummary
            migrationBuilder.UpdateData(table, keyColumn, 19, "AdminEmailTemplateId", "d-b320c2ae698e4ffaba882afb023d9a10"); // BookingAdministratorPaymentsSummary

            migrationBuilder.UpdateData(table, keyColumn, 20, "AgentEmailTemplateId", null); // BookingCancelledToReservations
            migrationBuilder.UpdateData(table, keyColumn, 21, "AgentEmailTemplateId", null); // BookingFinalizedToReservations

            migrationBuilder.UpdateData(table, keyColumn, 22, "AgentEmailTemplateId", null); // CreditCardPaymentReceivedAdministrator
            migrationBuilder.UpdateData(table, keyColumn, 22, "AdminEmailTemplateId", "d-689036bb7f324423808cf2dead06f7f4"); // CreditCardPaymentReceivedAdministrator

            migrationBuilder.UpdateData(table, keyColumn, 23, "AgentEmailTemplateId", null); // BookingManualCorrectionNeeded
            migrationBuilder.UpdateData(table, keyColumn, 23, "AdminEmailTemplateId", "d-e7e15134934a4824b04b20d175ca90c8"); // BookingManualCorrectionNeeded

            migrationBuilder.UpdateData(table, keyColumn, 32, "AgentEmailTemplateId", null); // PropertyOwnerBookingConfirmation
            migrationBuilder.UpdateData(table, keyColumn, 32, "PropertyOwnerEmailTemplateId", "d-3e37e6678d264729954ed08d7d01f290"); // PropertyOwnerBookingConfirmation
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminEmailTemplateId",
                table: "DefaultNotificationOptions");

            migrationBuilder.DropColumn(
                name: "PropertyOwnerEmailTemplateId",
                table: "DefaultNotificationOptions");

            migrationBuilder.RenameColumn(
                name: "AgentEmailTemplateId",
                table: "DefaultNotificationOptions",
                newName: "EmailTemplateId");
        }
    }
}
