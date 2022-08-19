using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class BookingVoucherPdfNotificationWasAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "AgentEmailTemplateId", "AdminEmailTemplateId", "EnabledReceivers" },
                new object[,]
                {
                    { (int)NotificationTypes.BookingVoucherPdf, 3, true, "d-725cfa3f77b042adb9b611487757de54", "d-107b7d5c536c4adabc63620649b90d1b", (int)ReceiverTypes.AgentApp }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
