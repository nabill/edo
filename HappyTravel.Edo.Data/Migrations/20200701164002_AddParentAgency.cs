using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddParentAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Agencies");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Agencies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Agencies");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Agencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
