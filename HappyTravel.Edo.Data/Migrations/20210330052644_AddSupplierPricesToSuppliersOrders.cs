using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddSupplierPricesToSuppliersOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SupplierOrders",
                newName: "PriceInUsd");

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierPrice",
                table: "SupplierOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SupplierCurrency",
                table: "SupplierOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierPrice",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "SupplierCurrency",
                table: "SupplierOrders");

            migrationBuilder.RenameColumn(
                name: "PriceInUsd",
                table: "SupplierOrders",
                newName: "Price");
        }
    }
}
