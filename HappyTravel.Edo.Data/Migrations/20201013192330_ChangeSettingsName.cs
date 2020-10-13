using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSettingsName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvailabilitySearchSettings",
                table: "AgentSystemSettings",
                newName: "AccommodationBookingSettings");

            migrationBuilder.RenameColumn(
                name: "AvailabilitySearchSettings",
                table: "AgencySystemSettings",
                newName: "AccommodationBookingSettings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccommodationBookingSettings",
                table: "AgentSystemSettings",
                newName: "AvailabilitySearchSettings");

            migrationBuilder.RenameColumn(
                name: "AccommodationBookingSettings",
                table: "AgencySystemSettings",
                newName: "AvailabilitySearchSettings");
        }
    }
}
