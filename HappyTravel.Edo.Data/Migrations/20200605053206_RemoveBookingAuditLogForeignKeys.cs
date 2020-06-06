using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveBookingAuditLogForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAuditLog_Customers_CustomerId",
                table: "BookingAuditLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
