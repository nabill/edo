using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveNotVerifiedAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "delete\nfrom \"PaymentAccounts\"\nwhere \"AgencyId\" in \n      (select ag.\"Id\" \n       from \"Agencies\" ag inner join \"Counterparties\" c on ag.\"CounterpartyId\" = ag.\"Id\"\n       where c.\"State\" != 1);");

            migrationBuilder.Sql(
                "delete \nfrom \"CounterpartyAccounts\"\nwhere \"CounterpartyId\" in\n      (select \"Id\"\n       from \"Counterparties\"\n       where \"State\" != 1);\n");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
