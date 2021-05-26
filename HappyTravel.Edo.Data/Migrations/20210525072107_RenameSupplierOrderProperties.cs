using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameSupplierOrderProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OriginalSupplierPrice",
                table: "SupplierOrders",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "OriginalSupplierCurrency",
                table: "SupplierOrders",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "ConvertedSupplierPrice",
                table: "SupplierOrders",
                newName: "ConvertedPrice");

            migrationBuilder.RenameColumn(
                name: "ConvertedSupplierCurrency",
                table: "SupplierOrders",
                newName: "ConvertedCurrency");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SupplierOrders",
                newName: "OriginalSupplierPrice");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "SupplierOrders",
                newName: "OriginalSupplierCurrency");

            migrationBuilder.RenameColumn(
                name: "ConvertedPrice",
                table: "SupplierOrders",
                newName: "ConvertedSupplierPrice");

            migrationBuilder.RenameColumn(
                name: "ConvertedCurrency",
                table: "SupplierOrders",
                newName: "ConvertedSupplierCurrency");
        }
    }
}
