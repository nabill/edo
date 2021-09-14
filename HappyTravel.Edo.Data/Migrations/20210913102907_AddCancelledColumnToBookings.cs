using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCancelledColumnToBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Cancelled",
                table: "Bookings",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"Bookings\" " +
                "SET \"Cancelled\" = \"BookingStatusHistory\".\"CreatedAt\"  FROM \"BookingStatusHistory\" " +
                "WHERE \"Bookings\".\"Id\" = \"BookingStatusHistory\".\"BookingId\" AND \"BookingStatusHistory\".\"Status\" = 4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancelled",
                table: "Bookings");
        }
    }
}
