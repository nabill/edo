using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddSupplierOrderDeadline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Deadline>(
                name: "Deadline",
                table: "SupplierOrders",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundableAmount",
                table: "SupplierOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
            
            var setRefundableAmountForCreatedOrdersSql = "UPDATE \"SupplierOrders\" SET \"RefundableAmount\" = 0 WHERE \"State\" = 1";
            migrationBuilder.Sql(setRefundableAmountForCreatedOrdersSql);
            
            var setRefundableAmountForCancelledOrdersSql = "UPDATE \"SupplierOrders\" SET \"RefundableAmount\" = \"Price\" WHERE \"State\" = 3";
            migrationBuilder.Sql(setRefundableAmountForCancelledOrdersSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "SupplierOrders");

            migrationBuilder.DropColumn(
                name: "RefundableAmount",
                table: "SupplierOrders");
        }
    }
}
