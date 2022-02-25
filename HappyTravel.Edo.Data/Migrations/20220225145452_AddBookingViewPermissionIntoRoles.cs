using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddBookingViewPermissionIntoRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var addPermissionSql =
                "update \"AdministratorRoles\" set \"Permissions\" = \"Permissions\" | 33554432 where \"Name\" = 'Booking manager' or \"Name\" = 'Finance manager';";
            migrationBuilder.Sql(addPermissionSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
