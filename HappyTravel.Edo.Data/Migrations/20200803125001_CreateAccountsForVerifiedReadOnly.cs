using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CreateAccountsForVerifiedReadOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createCounterpartyAccountsSql =
                "insert  into \"CounterpartyAccounts\" (\"CounterpartyId\", \"Balance\", \"Currency\", \"Created\")\nselect \"Id\", 0, 1, now()\nfrom \"Counterparties\" counterparty where counterparty.\"State\" = 3 and counterparty.\"Id\" not in (select \"CounterpartyId\" from \"CounterpartyAccounts\");\n";

            migrationBuilder.Sql(createCounterpartyAccountsSql);

            var createAgencyAccountsSql =
                "insert into \"AgencyAccounts\" (\"AgencyId\", \"Balance\", \"Currency\", \"Created\", \"AuthorizedBalance\") \nselect ag.\"Id\", 0, 1, now(), 0 from \"Agencies\" ag inner join \"Counterparties\" c on ag.\"CounterpartyId\" = c.\"Id\" where c.\"State\" = 3 and ag.\"Id\" not in (select \"AgencyId\" from \"AgencyAccounts\");";

            migrationBuilder.Sql(createAgencyAccountsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
