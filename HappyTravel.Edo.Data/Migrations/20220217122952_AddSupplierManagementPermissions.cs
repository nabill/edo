using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddSupplierManagementPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "update \"AdministratorRoles\" set \"Permissions\" = \"Permissions\" | 16777216 where \"Name\" = 'System Administrator';";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
