using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveVerificationData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Agencies\" a " +
                "SET \"Verified\" = (SELECT \"Verified\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\"), " +
                "\"ContractKind\" = (SELECT \"ContractKind\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\"), " +
                "\"VerificationReason\" = (SELECT \"VerificationReason\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\"), " +
                "\"VerificationState\" = (SELECT \"State\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\") " +
                "WHERE a.\"ParentId\" IS null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
