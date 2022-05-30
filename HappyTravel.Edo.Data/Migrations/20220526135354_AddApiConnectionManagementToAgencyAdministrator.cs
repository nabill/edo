using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddApiConnectionManagementToAgencyAdministrator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ApiConnectionManagement permission to Agency administrator
            migrationBuilder.Sql("update \"AgentRoles\" set \"Permissions\" = \"Permissions\" | 262144" +
                " where \"Name\" = 'Agency administrator';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
