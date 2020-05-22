using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class NumberOfNightsInBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfNights",
                table: "Bookings",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE \"Bookings\" SET \"NumberOfNights\" = DATE_PART('day', (\"CheckOutDate\"-\"CheckInDate\"));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfNights",
                table: "Bookings");
        }
    }
}
