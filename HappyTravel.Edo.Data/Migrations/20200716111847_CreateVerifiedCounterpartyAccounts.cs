using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CreateVerifiedCounterpartyAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Accounts for verified counterparties
            migrationBuilder.Sql(
                "insert  into \"CounterpartyAccounts\" (\"CounterpartyId\", \"Balance\", \"Currency\", \"Created\")\nselect \"Id\", 0, 1, now()\nfrom \"Counterparties\" counterparty\nwhere counterparty.\"State\" = 1\nand counterparty.\"Id\" not in (select \"CounterpartyId\" from \"CounterpartyAccounts\");");

            // Accounts for agencies in verified counterparties
            migrationBuilder.Sql(
                "insert into \"PaymentAccounts\" (\"AgencyId\", \"Balance\", \"CreditLimit\", \"Currency\", \"Created\", \"AuthorizedBalance\")\nselect ag.\"Id\", 0, 0, 1, now(), 0\nfrom \"Agencies\" ag inner join \"Counterparties\" c on ag.\"CounterpartyId\" = c.\"Id\"\nwhere c.\"State\" = 1 and ag.\"Id\" not in (select \"AgencyId\" from \"PaymentAccounts\");");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
