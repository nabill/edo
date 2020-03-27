using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveCardObsoleteColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsedForPayments",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                table: "CreditCards");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsedForPayments",
                table: "CreditCards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                table: "CreditCards",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
