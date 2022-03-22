using Microsoft.EntityFrameworkCore.Migrations;
using HappyTravel.Edo.Common.Enums.Administrators;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSystemAdministratorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add AdministratorManagement and MarkupManagement permission to System Administrator role
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"Permissions\" = \"Permissions\" | 131072 | 8" +
                " where \"Name\" = 'System Administrator';");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
