using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeBookingVoucherPdfReceiverType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DefaultNotificationOptions",
                "Type",
                (int)NotificationTypes.BookingVoucherPdf,
                "EnabledReceivers",
                (int)(ReceiverTypes.AdminPanel | ReceiverTypes.AgentApp));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
