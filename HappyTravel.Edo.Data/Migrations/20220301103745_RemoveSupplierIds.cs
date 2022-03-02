using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveSupplierIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierOrders_Supplier",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_SupplierCode",
                table: "SupplierOrders",
                column: "SupplierCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupplierOrders_SupplierCode",
                table: "SupplierOrders");

            migrationBuilder.AddColumn<int>(
                name: "Supplier",
                table: "SupplierOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Supplier",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrders_Supplier",
                table: "SupplierOrders",
                column: "Supplier");
        }
    }
}
