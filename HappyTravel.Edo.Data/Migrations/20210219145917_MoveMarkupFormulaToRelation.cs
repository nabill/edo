using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveMarkupFormulaToRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayedMarkupFormula",
                table: "Agents");

            migrationBuilder.AddColumn<string>(
                name: "DisplayedMarkupFormula",
                table: "AgentAgencyRelations",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayedMarkupFormula",
                table: "AgentAgencyRelations");

            migrationBuilder.AddColumn<string>(
                name: "DisplayedMarkupFormula",
                table: "Agents",
                type: "text",
                nullable: true);
        }
    }
}
