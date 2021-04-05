using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddConvertedPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupplierPrice",
                table: "SupplierOrders",
                newName: "OriginalSupplierPrice");
            
            migrationBuilder.RenameColumn(
                name: "SupplierCurrency",
                table: "SupplierOrders",
                newName: "OriginalSupplierCurrency");

            migrationBuilder.RenameColumn(
                name: "PriceInUsd",
                table: "SupplierOrders",
                newName: "ConvertedSupplierPrice");
            
            migrationBuilder.AddColumn<int>(
                name: "ConvertedSupplierCurrency",
                table: "SupplierOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginalSupplierPrice",
                table: "SupplierOrders",
                newName: "SupplierPrice");

            migrationBuilder.RenameColumn(
                name: "OriginalSupplierCurrency",
                table: "SupplierOrders",
                newName: "SupplierCurrency");

            migrationBuilder.RenameColumn(
                name: "ConvertedSupplierPrice",
                table: "SupplierOrders",
                newName: "PriceInUsd");

            migrationBuilder.DropColumn(
                name: "ConvertedSupplierCurrency",
                table: "SupplierOrders");
        }
    }
}
