using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class BookingWithDataProviders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DataProvider",
                table: "Bookings",
                nullable: false,
                defaultValue: 0);

            var setNetstormingProviderForBookings = "UPDATE public.\"Bookings\" SET \"DataProvider\" = 1";
            migrationBuilder.Sql(setNetstormingProviderForBookings);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataProvider",
                table: "Bookings");
        }
    }
}
