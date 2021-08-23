using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPaymentProcessorColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentProcessor",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE \"Payments\" " +
                "SET \"PaymentProcessor\" = 1 ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProcessor",
                table: "Payments");
        }
    }
}
