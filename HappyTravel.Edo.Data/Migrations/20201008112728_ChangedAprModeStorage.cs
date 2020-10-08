using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangedAprModeStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvancedPurchaseRatesSettings",
                table: "AgencySystemSettings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvancedPurchaseRatesSettings",
                table: "AgencySystemSettings",
                type: "integer",
                nullable: true);
        }
    }
}
