using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class BookingInfoNormalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingDetails",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PreviousBookingDetails",
                table: "BookingAuditLog");

            migrationBuilder.AddColumn<string>(
                name: "AccommodationId",
                table: "Bookings",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccommodationName",
                table: "Bookings",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationInfo",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Rooms",
                table: "Bookings",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccommodationId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AccommodationName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LocationInfo",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Rooms",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "BookingDetails",
                table: "Bookings",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousBookingDetails",
                table: "BookingAuditLog",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
