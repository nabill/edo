using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AccommodationDuplicateReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "AccommodationDuplicates",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentReportId",
                table: "AccommodationDuplicates",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccommodationDuplicateReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReporterAgentId = table.Column<int>(nullable: false),
                    ReporterAgencyId = table.Column<int>(nullable: false),
                    IsApproved = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDuplicateReports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicateReports_ReporterAgencyId",
                table: "AccommodationDuplicateReports",
                column: "ReporterAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicateReports_ReporterAgentId",
                table: "AccommodationDuplicateReports",
                column: "ReporterAgentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationDuplicateReports");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "AccommodationDuplicates");

            migrationBuilder.DropColumn(
                name: "ParentReportId",
                table: "AccommodationDuplicates");
        }
    }
}
