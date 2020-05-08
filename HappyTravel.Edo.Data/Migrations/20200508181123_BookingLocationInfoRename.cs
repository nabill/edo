using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class BookingLocationInfoRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationInfo",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "LocationInfo",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
