using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class HotAdminNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"DefaultNotificationOptions\" " +
                "set \"EnabledProtocols\" = 3, \"EnabledReceivers\" = 3, \"AdminEmailTemplateId\" = 'here will be template id' " +
                "where \"Type\" = 8");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
