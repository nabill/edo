using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixDefaultNotificationOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AccountBalanceManagementNotification
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "EnabledReceivers", (int)ReceiverTypes.AdminPanel);
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "AgentEmailTemplateId", null);
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 33, "AdminEmailTemplateId", "d-33da6c91aaef4b86bfa3ebba68f9d4bc");

            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{4, 19, 22, 33}' where \"Id\" = 2");
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{2, 6, 7, 18, 23, 34}' where \"Id\" = 3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
