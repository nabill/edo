using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddClientReferenceCodeColumnBookingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientReferenceCode",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClientReferenceCode",
                table: "Bookings",
                column: "ClientReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClientReferenceCode",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ClientReferenceCode",
                table: "Bookings");
        }
    }
}
