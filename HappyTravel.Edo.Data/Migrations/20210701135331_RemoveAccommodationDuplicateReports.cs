using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveAccommodationDuplicateReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationDuplicateReports");

            migrationBuilder.DropTable(
                name: "AccommodationDuplicates");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationDuplicateReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Accommodations = table.Column<List<SupplierAccommodationId>>(type: "jsonb", nullable: true),
                    ApprovalState = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EditorAdministratorId = table.Column<int>(type: "integer", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReporterAgencyId = table.Column<int>(type: "integer", nullable: false),
                    ReporterAgentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDuplicateReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationDuplicates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccommodationId1 = table.Column<string>(type: "text", nullable: true),
                    AccommodationId2 = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ParentReportId = table.Column<int>(type: "integer", nullable: false),
                    ReporterAgencyId = table.Column<int>(type: "integer", nullable: false),
                    ReporterAgentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDuplicates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicateReports_ReporterAgencyId",
                table: "AccommodationDuplicateReports",
                column: "ReporterAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDuplicateReports_ReporterAgentId",
                table: "AccommodationDuplicateReports",
                column: "ReporterAgentId");

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
    }
}
