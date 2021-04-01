using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddConvertedPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceInUsd",
                table: "SupplierOrders",
                newName: "ConvertedPrice");

            migrationBuilder.AddColumn<int>(
                name: "ConvertedCurrency",
                table: "SupplierOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedCurrency",
                table: "SupplierOrders");

            migrationBuilder.RenameColumn(
                name: "ConvertedPrice",
                table: "SupplierOrders",
                newName: "PriceInUsd");
        }
    }
}
