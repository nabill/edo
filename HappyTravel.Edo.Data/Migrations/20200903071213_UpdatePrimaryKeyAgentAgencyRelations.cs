using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdatePrimaryKeyAgentAgencyRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentAgencyRelations",
                table: "AgentAgencyRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentAgencyRelations",
                table: "AgentAgencyRelations",
                columns: new[] { "AgentId", "AgencyId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentAgencyRelations",
                table: "AgentAgencyRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentAgencyRelations",
                table: "AgentAgencyRelations",
                columns: new[] { "AgentId", "Type" });
        }
    }
}
