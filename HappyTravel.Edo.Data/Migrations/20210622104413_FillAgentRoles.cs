using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillAgentRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("AgentRoles", new string[] {"Name", "Permissions"}, new object[,]
            {
                {"Accounts manager", 64 | 128 | 256},
                {"Sales agent", 2 | 4 | 1024},
                {"Auditor", 4096 | 32 | 16 | 65536 | 16384},
                {"Agency administrator", 1 | 131072 | 8 | 2048 | 32768 | 8192},
            });

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = ARRAY[]::INTEGER[];");

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = \"AgentRoleIds\" || 4 " +
                "WHERE \"InAgencyPermissions\" & 64 > 0 OR \"InAgencyPermissions\" & 128 > 0 OR \"InAgencyPermissions\" & 256 > 0;");

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = \"AgentRoleIds\" || 3 " +
                "WHERE \"InAgencyPermissions\" & 2 > 0 OR \"InAgencyPermissions\" & 4 > 0 OR \"InAgencyPermissions\" & 1024 > 0;");

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = \"AgentRoleIds\" || 2 " +
                "WHERE \"InAgencyPermissions\" & 16 > 0 OR \"InAgencyPermissions\" & 32 > 0 OR \"InAgencyPermissions\" & 4096 > 0 " +
                "OR \"InAgencyPermissions\" & 16384 > 0 OR \"InAgencyPermissions\" & 65536 > 0;");

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" " +
                "SET \"AgentRoleIds\" = \"AgentRoleIds\" || 2 " +
                "WHERE \"InAgencyPermissions\" & 1 > 0 OR \"InAgencyPermissions\" & 8 > 0 OR \"InAgencyPermissions\" & 2048 > 0 " +
                "OR \"InAgencyPermissions\" & 8192 > 0 OR \"InAgencyPermissions\" & 32768 > 0 OR \"InAgencyPermissions\" & 131072 > 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
