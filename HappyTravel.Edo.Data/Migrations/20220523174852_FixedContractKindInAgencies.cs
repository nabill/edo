using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixedContractKindInAgencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 5 where \"ContractKind\" = 2");
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 2 where \"ContractKind\" = 3");
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 3 where \"ContractKind\" = 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 5 where \"ContractKind\" = 3");
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 3 where \"ContractKind\" = 2");
            migrationBuilder.Sql("Update \"Agencies\" Set \"ContractKind\" = 2 where \"ContractKind\" = 5");
        }
    }
}
