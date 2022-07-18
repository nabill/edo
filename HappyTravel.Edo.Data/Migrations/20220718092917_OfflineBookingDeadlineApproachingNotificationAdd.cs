using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class OfflineBookingDeadlineApproachingNotificationAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "AgentEmailTemplateId", "EnabledReceivers" },
                new object[,]
                {
                    { (int)NotificationTypes.OfflineBookingDeadlineApproaching, 3, true, "d-e5fea914c416484db417a5336f45a743", (int)ReceiverTypes.AgentApp }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
