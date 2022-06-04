using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPermissionToSeePaxNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ViewPaxNames permission to Booking manager
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"Permissions\" = \"Permissions\" | 134217728" +
                " where \"Name\" = 'Booking manager';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
