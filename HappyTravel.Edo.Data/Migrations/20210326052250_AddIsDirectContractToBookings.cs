using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddIsDirectContractToBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDirectContract",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_IsDirectContract",
                table: "Bookings",
                column: "IsDirectContract");

            migrationBuilder.Sql("UPDATE \"Bookings\" SET \"IsDirectContract\" = true WHERE 'direct-connectivity' = ANY(\"Tags\")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_IsDirectContract",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsDirectContract",
                table: "Bookings");
        }
    }
}
