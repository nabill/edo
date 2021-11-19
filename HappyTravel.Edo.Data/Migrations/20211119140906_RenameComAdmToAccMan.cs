using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameComAdmToAccMan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Name\" = 'Account manager' " +
                "where \"Id\" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
