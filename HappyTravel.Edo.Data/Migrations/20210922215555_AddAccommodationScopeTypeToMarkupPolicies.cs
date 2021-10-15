using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAccommodationScopeTypeToMarkupPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccommodationScopeId",
                table: "MarkupPolicies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccommodationScopeType",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccommodationScopeId",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "AccommodationScopeType",
                table: "MarkupPolicies");
        }
    }
}
