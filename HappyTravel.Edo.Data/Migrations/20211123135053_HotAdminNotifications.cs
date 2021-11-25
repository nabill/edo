using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class HotAdminNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"DefaultNotificationOptions\" " +
                "set \"EnabledProtocols\" = 3, \"EnabledReceivers\" = 3, \"AdminEmailTemplateId\" = 'd-4a5ae6b1ffe8437a95d211ac12c053f3' " +
                "where \"Type\" = 8");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
