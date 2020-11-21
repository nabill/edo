using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameDataProviderColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierOrders_DataProvider",
                table: "SupplierOrders");

            migrationBuilder.RenameColumn(name: "DataProvider",
                table: "SupplierOrders",
                newName: "Supplier");

            migrationBuilder.RenameColumn(name: "DataProviders",
                table: "Locations",
                newName: "Suppliers");


            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_Supplier",
                table: "SupplierOrders",
                column: "Supplier");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierOrders_Supplier",
                table: "SupplierOrders");
            
            migrationBuilder.RenameColumn(name: "Supplier",
                table: "SupplierOrders",
                newName: "DataProvider");
            
            migrationBuilder.RenameColumn(name: "Suppliers",
                table: "Locations",
                newName: "DataProviders");
            
            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_DataProvider",
                table: "SupplierOrders",
                column: "DataProvider");
        }
    }
}
