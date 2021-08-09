using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixSqlQueryFromPrevMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Counterparties\" c " +
                "SET \"Address\" = a.\"Address\", \"BillingEmail\" = a.\"BillingEmail\", \"City\" = a.\"City\", " +
                    "\"CountryCode\" = a.\"CountryCode\", \"Fax\" = a.\"Fax\", \"Phone\" = a.\"Phone\", " +
                    "\"PostalCode\" = a.\"PostalCode\", \"Website\" = a.\"Website\", \"VatNumber\" = a.\"VatNumber\" " +
                "FROM \"Agencies\" a " +
                "WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
