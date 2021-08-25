using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeBookingStatusForChargingProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Bookings\" " +
                "SET \"Status\" = 3 " +
                "WHERE \"Id\" IN (660, 719, 663, 713);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Bookings\" " +
                "SET \"Status\" = 8 " +
                "WHERE \"Id\" IN (660, 719, 663, 713);");
        }
    }
}
