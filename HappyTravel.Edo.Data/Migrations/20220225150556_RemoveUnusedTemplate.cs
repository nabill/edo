using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveUnusedTemplate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DefaultNotificationOptions", "Type", 4, "AdminEmailTemplateId", null); // SuccessfulPaymentReceipt
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
