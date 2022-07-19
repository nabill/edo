using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixOfflineBookingDeadlineApproachingNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefaultNotificationOptions",
                keyColumn: "Type",
                keyValue: (int)NotificationTypes.OfflineBookingDeadlineApproaching,
                column: "AgentEmailTemplateId",
                value: "d-182729d281c14c17af76bccf87b91365");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
