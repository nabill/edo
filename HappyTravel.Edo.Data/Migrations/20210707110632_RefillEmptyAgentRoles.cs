using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RefillEmptyAgentRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = '{1, 2, 3, 4}' " +
                "WHERE \"InAgencyPermissions\" = 2147483647 AND \"AgentRoleIds\" IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
