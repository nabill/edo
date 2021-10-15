using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCountryHtIdLocalityHtIdToAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryHtId",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalityHtId",
                table: "Agencies",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryHtId",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "LocalityHtId",
                table: "Agencies");
        }
    }
}
