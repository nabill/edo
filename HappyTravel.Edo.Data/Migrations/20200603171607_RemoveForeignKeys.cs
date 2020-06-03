using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAuditLog_Agents_AgentId",
                table: "BookingAuditLog");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingAuditLog_Bookings_BookingId",
                table: "BookingAuditLog");

            migrationBuilder.DropIndex(
                name: "IX_BookingAuditLog_AgentId",
                table: "BookingAuditLog");

            migrationBuilder.DropIndex(
                name: "IX_BookingAuditLog_BookingId",
                table: "BookingAuditLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BookingAuditLog_AgentId",
                table: "BookingAuditLog",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAuditLog_BookingId",
                table: "BookingAuditLog",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAuditLog_Agents_AgentId",
                table: "BookingAuditLog",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAuditLog_Bookings_BookingId",
                table: "BookingAuditLog",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
