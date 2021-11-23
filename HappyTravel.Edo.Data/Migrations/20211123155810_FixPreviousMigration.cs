using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixPreviousMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var table = "DefaultNotificationOptions";
            var keyColumn = "Type";
            migrationBuilder.UpdateData(table, keyColumn, 8, "EnabledProtocols", (int)ProtocolTypes.WebSocket);
            migrationBuilder.UpdateData(table, keyColumn, 8, "AdminEmailTemplateId", null);


            migrationBuilder.InsertData(table, new string[] { "Type", "EnabledProtocols", "IsMandatory", "EnabledReceivers", "AgentEmailTemplateId", "AdminEmailTemplateId", "PropertyOwnerEmailTemplateId" },
                new object[,]
                {
                    { (int)NotificationTypes.BookingStatusChangedToPendingOrWaitingForResponse, (int)(ProtocolTypes.Email), false, (int)ReceiverTypes.AdminPanel }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
