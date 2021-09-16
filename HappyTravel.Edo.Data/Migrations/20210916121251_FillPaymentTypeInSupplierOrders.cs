using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillPaymentTypeInSupplierOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""SupplierOrders"" SET ""PaymentType"" = CASE WHEN EXISTS (
                    SELECT * FROM ""Bookings"" WHERE 
                        ""Bookings"".""ReferenceCode"" = ""SupplierOrders"".""ReferenceCode"" AND
                        ""Bookings"".""Supplier"" = 8 AND
                        ARRAY_TO_STRING(""Tags"", ',') ILIKE '% vcc %'
                ) THEN 2 ELSE 1 END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""SupplierOrders"" SET ""PaymentType"" = 0");
        }
    }
}
