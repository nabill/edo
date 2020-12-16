using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPaidIndexInAppliedMarkups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppliedBookingMarkups_Paid",
                table: "AppliedBookingMarkups",
                column: "Paid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppliedBookingMarkups_Paid",
                table: "AppliedBookingMarkups");
        }
    }
}
