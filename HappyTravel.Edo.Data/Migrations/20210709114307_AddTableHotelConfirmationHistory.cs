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
                name: "PropertyOwnerConfirmationCode",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookingConfirmationHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    ConfirmationCode = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    Initiator = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingConfirmationHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingConfirmationHistory_CreatedAt",
                table: "BookingConfirmationHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingConfirmationHistory_ReferenceCode",
                table: "BookingConfirmationHistory",
                column: "ReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingConfirmationHistory");

            migrationBuilder.DropColumn(
                name: "PropertyOwnerConfirmationCode",
                table: "Bookings");
        }
    }
}
