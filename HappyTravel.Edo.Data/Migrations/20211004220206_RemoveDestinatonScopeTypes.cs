using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveDestinatonScopeTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationScopeType",
                table: "MarkupPolicies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationScopeType",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
