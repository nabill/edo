using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class GivePaymentAccountsToAgencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update public.\"PaymentAccounts\" as t " +
                "set \"CounterpartyId\" = coalesce( " +
                "                                (select a.\"Id\" " +
                "                                 from public.\"Agencies\" a " +
                "                                 where a.\"IsDefault\" = true " +
                "                                   and a.\"CounterpartyId\" = t.\"CounterpartyId\"), -1);");

            migrationBuilder.RenameColumn(
                name: "CounterpartyId",
                newName: "AgencyId",
                table: "PaymentAccounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AgencyId",
                newName: "CounterpartyId",
                table: "PaymentAccounts");

            migrationBuilder.Sql("update public.\"PaymentAccounts\" as t " +
                "set \"CounterpartyId\" = coalesce( " +
                "                                (select a.\"CounterpartyId\" " +
                "                                 from public.\"Agencies\" a " +
                "                                 where a.\"IsDefault\" = true " +
                "                                   and a.\"Id\" = t.\"CounterpartyId\"), -1);");
        }
    }
}
