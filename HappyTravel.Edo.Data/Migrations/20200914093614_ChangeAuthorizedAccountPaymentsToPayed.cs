using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeAuthorizedAccountPaymentsToPayed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizedBalance",
                table: "AgencyAccounts");

            migrationBuilder.Sql("update public.\"Payments\" set \"Status\" = 4 where \"PaymentMethod\" = 1 and \"Status\" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AuthorizedBalance",
                table: "AgencyAccounts",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
