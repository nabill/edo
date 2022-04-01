using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameRegionFieldsInFewEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "Countries",
                newName: "MarketId");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "Agencies",
                newName: "MarketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MarketId",
                table: "Countries",
                newName: "RegionId");

            migrationBuilder.RenameColumn(
                name: "MarketId",
                table: "Agencies",
                newName: "RegionId");
        }
    }
}
