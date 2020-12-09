using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddMarkupBookingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarkupLog");

            migrationBuilder.CreateTable(
                name: "BookingMarkups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceCode = table.Column<string>(type: "text", nullable: true),
                    PolicyId = table.Column<int>(type: "integer", nullable: false),
                    ScopeType = table.Column<int>(type: "integer", nullable: false),
                    ScopeId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    AgentId = table.Column<int>(type: "integer", nullable: false),
                    AgencyId = table.Column<int>(type: "integer", nullable: false),
                    CounterpartyId = table.Column<int>(type: "integer", nullable: false),
                    PayedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingMarkups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarkupPaymentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgencyAccountId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    BookingMarkupId = table.Column<int>(type: "integer", nullable: false),
                    ReferenceCode = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupPaymentLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingMarkups_ReferenceCode",
                table: "BookingMarkups",
                column: "ReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingMarkups");

            migrationBuilder.DropTable(
                name: "MarkupPaymentLogs");

            migrationBuilder.CreateTable(
                name: "MarkupLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Policies = table.Column<string>(type: "jsonb", nullable: false),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarkupLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarkupLog_ReferenceCode",
                table: "MarkupLog",
                column: "ReferenceCode");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupLog_ServiceType",
                table: "MarkupLog",
                column: "ServiceType");
        }
    }
}
