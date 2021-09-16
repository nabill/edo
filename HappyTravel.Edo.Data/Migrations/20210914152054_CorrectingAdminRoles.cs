using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CorrectingAdminRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AdministratorRoles\" " +
                "SET \"Name\" = 'Account manager' " +
                "WHERE \"Name\" = 'Accounts manager' ");

            migrationBuilder.Sql("UPDATE \"AdministratorRoles\" " +
                "SET \"Permissions\" = \"Permissions\" & ~1024 " +
                "WHERE \"Name\" = 'Booking manager' ");

            migrationBuilder.Sql("DELETE FROM \"AdministratorRoles\" " +
                "WHERE \"Name\" = 'Test role' ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
