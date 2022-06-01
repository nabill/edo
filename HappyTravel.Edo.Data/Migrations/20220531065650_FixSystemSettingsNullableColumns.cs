using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixSystemSettingsNullableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<AgentAccommodationBookingSettings>(
                name: "AccommodationBookingSettings",
                table: "AgentSystemSettings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(AgentAccommodationBookingSettings),
                oldType: "jsonb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
