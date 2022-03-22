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
                
            // Add notification types to super admin role
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{2,6,7,16,17,18,19,22,23,33,34}'" +
                " where \"Name\" = 'Super-admin';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
