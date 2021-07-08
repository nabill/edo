using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddTableHotelConfirmationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationCode",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HotelConfirmationHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    ConfirmationCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Initiator = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelConfirmationHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelConfirmationHistory_CreatedAt",
                table: "HotelConfirmationHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HotelConfirmationHistory_ReferenceCode",
                table: "HotelConfirmationHistory",
                column: "ReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotelConfirmationHistory");

            migrationBuilder.DropColumn(
                name: "ConfirmationCode",
                table: "Bookings");
        }
    }
}
