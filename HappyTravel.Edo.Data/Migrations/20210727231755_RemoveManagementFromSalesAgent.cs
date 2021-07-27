using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveManagementFromSalesAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" & ~1024 " +
                "WHERE \"Name\" ILIKE 'Sales agent'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" | 1024 " +
                "WHERE \"Name\" ILIKE 'Sales agent'");
        }
    }
}
