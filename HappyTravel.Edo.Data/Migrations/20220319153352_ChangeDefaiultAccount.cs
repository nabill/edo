using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeDefaiultAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("CompanyAccounts", "Id", 3, "IsDefault", true);
            migrationBuilder.UpdateData("CompanyAccounts", "Id", 4, "IsDefault", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
