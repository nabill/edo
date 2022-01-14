using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPaymentLinkManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("AdministratorRoles", new string[] {"Name", "Permissions"}, new object[,]
            {
                {"Payment link manager", 512},
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"AdministratorRoles\" WHERE \"Name\" = 'Payment link manager'");
        }
    }
}
