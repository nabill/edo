using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.AccommodationMappings;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AccommodationDuplicateReportsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationDuplicateReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Accommodation = table.Column<ProviderAccommodationId>(type: "jsonb", nullable: true),
                    Duplicates = table.Column<List<ProviderAccommodationId>>(type: "jsonb", nullable: true),
                    ReporterAgentId = table.Column<int>(nullable: false),
                    ReporterAgencyId = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDuplicateReports", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationDuplicateReports");
        }
    }
}
