using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddedDisplayedMarkupFormulaTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayedMarkupFormula",
                table: "AgentAgencyRelations");

            migrationBuilder.CreateTable(
                name: "DisplayMarkupFormulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CounterpartyId = table.Column<int>(type: "integer", nullable: false),
                    AgencyId = table.Column<int>(type: "integer", nullable: true),
                    AgentId = table.Column<int>(type: "integer", nullable: true),
                    DisplayFormula = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayMarkupFormulas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayMarkupFormulas_CounterpartyId_AgencyId_AgentId",
                table: "DisplayMarkupFormulas",
                columns: new[] { "CounterpartyId", "AgencyId", "AgentId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayMarkupFormulas");

            migrationBuilder.AddColumn<string>(
                name: "DisplayedMarkupFormula",
                table: "AgentAgencyRelations",
                type: "text",
                nullable: true);
        }
    }
}
