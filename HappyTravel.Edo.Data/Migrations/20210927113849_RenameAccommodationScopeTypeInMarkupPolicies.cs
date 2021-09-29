using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameAccommodationScopeTypeInMarkupPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccommodationScopeType",
                table: "MarkupPolicies",
                newName: "DestinationScopeType");

            migrationBuilder.RenameColumn(
                name: "AccommodationScopeId",
                table: "MarkupPolicies",
                newName: "DestinationScopeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DestinationScopeType",
                table: "MarkupPolicies",
                newName: "AccommodationScopeType");

            migrationBuilder.RenameColumn(
                name: "DestinationScopeId",
                table: "MarkupPolicies",
                newName: "AccommodationScopeId");
        }
    }
}
