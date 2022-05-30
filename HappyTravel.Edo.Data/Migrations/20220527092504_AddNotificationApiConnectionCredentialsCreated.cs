using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddNotificationApiConnectionCredentialsCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("DefaultNotificationOptions", new string[] { "Type", "EnabledProtocols", "IsMandatory", "AdminEmailTemplateId", "EnabledReceivers" },
                new object[,]
                {
                    { (int)NotificationTypes.ApiConnectionCredentialsCreated, (int)(ProtocolTypes.Email), false, "d-e5fea914c416484db417a5336f45a743", (int)ReceiverTypes.AdminPanel }
                });
            
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{37}' where \"Name\" = 'System Administrator'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
