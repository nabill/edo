using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AccommodationDuplicatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationDuplicates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccommodationId1 = table.Column<string>(nullable: true),
                    AccommodationId2 = table.Column<string>(nullable: true),
                    ReporterAgentId = table.Column<int>(nullable: false),
                    ReporterAgencyId = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDuplicates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicates_AccommodationId1",
                table: "AccommodationDuplicates",
                column: "AccommodationId1");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicates_AccommodationId2",
                table: "AccommodationDuplicates",
                column: "AccommodationId2");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicates_ReporterAgencyId",
                table: "AccommodationDuplicates",
                column: "ReporterAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicates_ReporterAgentId",
                table: "AccommodationDuplicates",
                column: "ReporterAgentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationDuplicates");
        }
    }
}
