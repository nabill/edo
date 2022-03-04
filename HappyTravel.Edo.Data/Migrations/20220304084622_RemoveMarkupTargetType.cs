using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveMarkupTargetType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_Target",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "MarkupPolicies");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Target",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_Target",
                table: "MarkupPolicies",
                column: "Target");
        }
    }
}
