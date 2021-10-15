using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddSupplierPaymentOrderColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "SupplierOrders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.Sql("UPDATE \"SupplierOrders\" so " +
            "SET \"PaymentDate\" = CASE WHEN EXISTS (" +
                "SELECT * FROM \"Bookings\" b " +
                "WHERE EXISTS (" +
                "SELECT FROM jsonb_array_elements(b.\"Rooms\") rooms " +
            "WHERE (rooms->'isAdvancePurchaseRate')::boolean is true AND b.\"ReferenceCode\" = so.\"ReferenceCode\"" +
                ")" +
                ") THEN b.\"Created\" ELSE b.\"CheckOutDate\" END " +
                "FROM \"Bookings\" b " +
                "WHERE b.\"ReferenceCode\" = so.\"ReferenceCode\";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "SupplierOrders");
        }
    }
}
