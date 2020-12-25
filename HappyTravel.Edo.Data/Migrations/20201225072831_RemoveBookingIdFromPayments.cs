using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveBookingIdFromPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReferenceCode",
                table: "Payments",
                column: "ReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_ReferenceCode",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "Payments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");
        }
    }
}
