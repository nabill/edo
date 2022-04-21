using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class InsertNotificationAndAddedItToAccountManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DefaultNotificationOptions",
                columns: new[] { "Type", "EnabledProtocols", "IsMandatory", "EnabledReceivers",
                    "AgentEmailTemplateId", "AdminEmailTemplateId", "PropertyOwnerEmailTemplateId" },
                values: new object[,]
                {
                    { NotificationTypes.MarkupSetUpOrChanged, 3, false, 1, null, "d-1fffdcff817f4ec395b244d600ab832e", null }
                });

            migrationBuilder.UpdateData(
                table: "AdministratorRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "NotificationTypes",
                value: new NotificationTypes[] {
                    NotificationTypes.AdministratorInvitation, NotificationTypes.MasterAgentSuccessfulRegistration,
                    NotificationTypes.MarkupSetUpOrChanged
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
