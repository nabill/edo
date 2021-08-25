using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveManagementFromSalesAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //revoke from Sales agent
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" & ~1024 " +
                "WHERE \"Name\" ILIKE 'Sales agent'");

            //grant to Agency administrator
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" | 1024 " +
                "WHERE \"Name\" ILIKE 'Agency administrator'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //grant to Sales agent
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" | 1024 " +
                "WHERE \"Name\" ILIKE 'Sales agent'");

            //revoke from Agency administrator
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"Permissions\" = \"Permissions\" & ~1024 " +
                "WHERE \"Name\" ILIKE 'Agency administrator'");
        }
    }
}
