using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveCounterpartyFromRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentCounterpartyRelations",
                table: "AgentCounterpartyRelations");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "AgentCounterpartyRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentCounterpartyRelations",
                table: "AgentCounterpartyRelations",
                columns: new[] { "AgentId", "AgencyId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentCounterpartyRelations",
                table: "AgentCounterpartyRelations");

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyId",
                table: "AgentCounterpartyRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentCounterpartyRelations",
                table: "AgentCounterpartyRelations",
                columns: new[] { "AgentId", "CounterpartyId", "Type" });
        }
    }
}
