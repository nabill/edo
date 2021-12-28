using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveCounterparty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counterparties");

            migrationBuilder.DropIndex(
                name: "IX_DisplayMarkupFormulas_CounterpartyId_AgencyId_AgentId",
                table: "DisplayMarkupFormulas");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CounterpartyId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Agencies_CounterpartyId",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "DisplayMarkupFormulas");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "Agencies");

            migrationBuilder.CreateIndex(
                name: "IX_DisplayMarkupFormulas_AgencyId_AgentId",
                table: "DisplayMarkupFormulas",
                columns: new[] { "AgencyId", "AgentId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DisplayMarkupFormulas_AgencyId_AgentId",
                table: "DisplayMarkupFormulas");

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyId",
                table: "DisplayMarkupFormulas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyId",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Counterparties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsContractUploaded = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counterparties", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayMarkupFormulas_CounterpartyId_AgencyId_AgentId",
                table: "DisplayMarkupFormulas",
                columns: new[] { "CounterpartyId", "AgencyId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CounterpartyId",
                table: "Bookings",
                column: "CounterpartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_CounterpartyId",
                table: "Agencies",
                column: "CounterpartyId");
        }
    }
}
