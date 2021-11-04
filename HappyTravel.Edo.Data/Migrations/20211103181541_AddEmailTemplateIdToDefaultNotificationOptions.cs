using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddEmailTemplateIdToDefaultNotificationOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailTemplateId",
                table: "DefaultNotificationOptions",
                type: "text",
                nullable: true);

            var table = "DefaultNotificationOptions";
            var keyColumn = "Type";
            var column = "EmailTemplateId";

            migrationBuilder.UpdateData(table, keyColumn, 1, column, "d-3e695c9b10ab4e76ba2a1b0611ce2f5b"); // BookingVoucher
            migrationBuilder.UpdateData(table, keyColumn, 2, column, "d-9baff8dd811749e0afc1ffb80f0c9f1d"); // BookingInvoice
            migrationBuilder.UpdateData(table, keyColumn, 3, column, "d-1e69b16152ef45f08426574e19ee8d3c"); // DeadlineApproaching
            migrationBuilder.UpdateData(table, keyColumn, 4, column, "d-646b7c9a3b61442c9327b43ec77d9ac5"); // SuccessfulPaymentReceipt
            migrationBuilder.UpdateData(table, keyColumn, 6, column, "d-da2c6d74ecb0466283f2b5a8116fba0f"); // BookingCancelled
            migrationBuilder.UpdateData(table, keyColumn, 7, column, "d-ebfa965b92cd431494258bd6b5a8f078"); // BookingFinalized
            migrationBuilder.UpdateData(table, keyColumn, 9, column, "d-bbcc2c6cd2234441a6153da4b25a691f"); // CreditCardPaymentReceived
            migrationBuilder.UpdateData(table, keyColumn, 10, column, "d-80262a7a65f645e3b8f9b06a79669588"); // CounterpartyAccountBalanceReplenished
            migrationBuilder.UpdateData(table, keyColumn, 11, column, "d-34ddd01a2ea440e18471c6fcb78b63a6"); // AgentInvitation
            migrationBuilder.UpdateData(table, keyColumn, 12, column, "d-78c5c9cf6e0c441eaf29f740712a1085"); // ChildAgencyInvitation
            migrationBuilder.UpdateData(table, keyColumn, 13, column, "d-92c515648e9a406ba7232c206a29512f"); // AgentSuccessfulRegistration
            migrationBuilder.UpdateData(table, keyColumn, 14, column, "d-4338bc0440724d5f86cbde8131fdcbc4"); // ChildAgencySuccessfulRegistration
            migrationBuilder.UpdateData(table, keyColumn, 15, column, "d-6f1bf8ffade944a5b9a003b7000c0fea"); // AgencyActivityChanged
            migrationBuilder.UpdateData(table, keyColumn, 16, column, "d-579c74eedd3a4624bd2bcd87535e49f0"); // AdministratorInvitation
            migrationBuilder.UpdateData(table, keyColumn, 17, column, "d-25ff2449ce9949e0aadb7bde35d6f2dc"); // MasterAgentSuccessfulRegistration
            migrationBuilder.UpdateData(table, keyColumn, 18, column, "d-0e0a051dfa62481aa54e3e3bc2ee7d0b"); // BookingsAdministratorSummaryNotification
            migrationBuilder.UpdateData(table, keyColumn, 19, column, "d-b320c2ae698e4ffaba882afb023d9a10"); // BookingAdministratorPaymentsSummary
            migrationBuilder.UpdateData(table, keyColumn, 20, column, "d-378dbbbe87194aa1bb7dfa1338169739"); // BookingCancelledToReservations
            migrationBuilder.UpdateData(table, keyColumn, 21, column, "d-9addc6e97b154ce9afdec1305e5eeddf"); // BookingFinalizedToReservations
            migrationBuilder.UpdateData(table, keyColumn, 22, column, "d-689036bb7f324423808cf2dead06f7f4"); // CreditCardPaymentReceivedAdministrator
            migrationBuilder.UpdateData(table, keyColumn, 23, column, "d-e7e15134934a4824b04b20d175ca90c8"); // BookingManualCorrectionNeeded
            migrationBuilder.UpdateData(table, keyColumn, 24, column, "d-6c99ec77db8d47ada2c60477e901116b"); // BookingSummaryReportForAgent
            migrationBuilder.UpdateData(table, keyColumn, 25, column, "d-e48c6cc90bbe4b5599aba10c4384c799"); // ExternalPaymentLinks
            migrationBuilder.UpdateData(table, keyColumn, 26, column, "d-5259343712ee4f61923ebd8ae4abafaf"); // PaymentLinkPaidNotification
            migrationBuilder.UpdateData(table, keyColumn, 27, column, "d-d0bbb6c268514988bb4b6416c376426e"); // CounterpartyActivityChanged
            migrationBuilder.UpdateData(table, keyColumn, 28, column, "d-986ff7d5cfc1472397344b15247b8f39"); // AgencyVerificationChanged
            migrationBuilder.UpdateData(table, keyColumn, 29, column, "d-3ca1f612ebab47f98c9e2bb7a17b203e"); // CounterpartyAccountBalanceSubtracted
            migrationBuilder.UpdateData(table, keyColumn, 30, column, "d-a96b230afd35483790ce554d6e85e345"); // CounterpartyAccountBalanceIncreasedManually
            migrationBuilder.UpdateData(table, keyColumn, 31, column, "d-2cbe406e5bf1481986007ecda6446fbc"); // CounterpartyAccountBalanceDecreasedManually
            migrationBuilder.UpdateData(table, keyColumn, 32, column, "d-3e37e6678d264729954ed08d7d01f290"); // PropertyOwnerBookingConfirmation
            migrationBuilder.UpdateData(table, keyColumn, 33, column, "d-33da6c91aaef4b86bfa3ebba68f9d4bc"); // AccountBalanceManagementNotification
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTemplateId",
                table: "DefaultNotificationOptions");
        }
    }
}
