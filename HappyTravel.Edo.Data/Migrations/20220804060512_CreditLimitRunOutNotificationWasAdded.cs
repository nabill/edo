using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CreditLimitRunOutNotificationWasAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditLimitNotifications",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "AgentEmailTemplateId", "EnabledReceivers" },
                new object[,]
                {
                    { (int)NotificationTypes.CreditLimitRunOutBalance, 3, true, "d-36cbedb258f64423a773f34484db4835", (int)ReceiverTypes.AgentApp }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditLimitNotifications",
                table: "Agencies");
        }
    }
}
