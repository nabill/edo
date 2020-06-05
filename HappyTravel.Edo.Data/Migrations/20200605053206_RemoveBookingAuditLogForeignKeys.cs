using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveBookingAuditLogForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            try
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_BookingAuditLog_Bookings_BookingId",
                    table: "BookingAuditLog");
            }
            catch (Exception ex)
            {
                // Nothing to do there, FK may exist
                Console.WriteLine(ex);
            }
            
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAuditLog_Customers_CustomerId",
                table: "BookingAuditLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
