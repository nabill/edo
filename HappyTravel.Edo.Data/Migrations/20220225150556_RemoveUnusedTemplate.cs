using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveUnusedTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SuccessfulPaymentReceipt
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 4, "EnabledReceivers", 2);
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 4, "AdminEmailTemplateId", null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
