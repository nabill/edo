using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAgentDirectApiClientRelationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentDirectApiClientRelations",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    DirectApiClientId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentDirectApiClientRelations", x => new { x.AgentId, x.DirectApiClientId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentDirectApiClientRelations");
        }
    }
}
