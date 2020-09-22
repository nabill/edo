using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeBookingDateToNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "BookingDate",
                table: "Bookings",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            var setDateNullSql = "UPDATE \"Bookings\" SET \"BookingDate\" = NULL WHERE \"BookingDate\" = '0001-01-01 00:00:00.000000'";
            migrationBuilder.Sql(setDateNullSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var setDefaultDateSql = "UPDATE \"Bookings\" SET \"BookingDate\" = '0001-01-01 00:00:00.000000' WHERE \"BookingDate\" IS NULL";
            migrationBuilder.Sql(setDefaultDateSql);
            
            migrationBuilder.AlterColumn<DateTime>(
                name: "BookingDate",
                table: "Bookings",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
