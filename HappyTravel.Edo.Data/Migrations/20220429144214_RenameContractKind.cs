using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameContractKind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 5 where \"PreferredPaymentMethod\" = 1");
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 1 where \"PreferredPaymentMethod\" = 3");
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 3 where \"PreferredPaymentMethod\" = 5");
            
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 5 where \"PaymentType\" = 1");
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 1 where \"PaymentType\" = 3");
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 3 where \"PaymentType\" = 5");
            
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 5 where \"PaymentMethod\" = 1");
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 1 where \"PaymentMethod\" = 3");
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 3 where \"PaymentMethod\" = 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 5 where \"PreferredPaymentMethod\" = 3");
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 3 where \"PreferredPaymentMethod\" = 1");
            migrationBuilder.Sql("Update \"Agencies\" Set \"PreferredPaymentMethod\" = 1 where \"PreferredPaymentMethod\" = 5");
            
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 5 where \"PaymentType\" = 3");
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 3 where \"PaymentType\" = 1");
            migrationBuilder.Sql("Update \"Bookings\" Set \"PaymentType\" = 1 where \"PaymentType\" = 5");
            
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 5 where \"PaymentMethod\" = 3");
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 3 where \"PaymentMethod\" = 1");
            migrationBuilder.Sql("Update \"Payments\" Set \"PaymentMethod\" = 1 where \"PaymentMethod\" = 5");
        }
    }
}
