using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixDefaultNotificationOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AccountBalanceManagementNotification
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "EnabledReceivers", 1);
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "AgentEmailTemplateId", null);
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "AdminEmailTemplateId", "d-33da6c91aaef4b86bfa3ebba68f9d4bc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
