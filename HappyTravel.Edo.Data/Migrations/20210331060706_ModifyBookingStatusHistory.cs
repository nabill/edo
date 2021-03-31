using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ModifyBookingStatusHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChangeSource",
                table: "BookingStatusHistory",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "ChangeReason",
                table: "BookingStatusHistory",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "ChangeEvent",
                table: "BookingStatusHistory",
                newName: "Event");

            migrationBuilder.AddColumn<int>(
                name: "Initiator",
                table: "BookingStatusHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Initiator",
                table: "BookingStatusHistory");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "BookingStatusHistory",
                newName: "ChangeSource");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "BookingStatusHistory",
                newName: "ChangeReason");

            migrationBuilder.RenameColumn(
                name: "Event",
                table: "BookingStatusHistory",
                newName: "ChangeEvent");
        }
    }
}
