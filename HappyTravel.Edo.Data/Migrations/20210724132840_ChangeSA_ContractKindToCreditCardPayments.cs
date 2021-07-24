using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSA_ContractKindToCreditCardPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Counterparties\" " +
                "SET \"ContractKind\" = 3 " +
                "WHERE \"Id\" IN (85, 86, 88, 89, 90, 91, 92, 93, 94, 102, 104, 105, 106, 109, 110, 111, 113, 115, 117, 118, 119, 120, 121, 124, 127, 131, 132, 133, 134, 136, 137, 142, 143, 145, 147, 148, 150, 152, 153, 156, 157, 159, 187, 211);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Counterparties\" " +
                "SET \"ContractKind\" = 1 " +
                "WHERE \"Id\" IN (86, 89 ,93, 105, 120, 91, 88, 94, 102, 104, 110, 111, 113, 115, 117, 118, 119, 121, 124, 127, 131, 132, 133, 134, 136, 137, 142, 143, 145, 147, 148, 150, 152, 153, 156, 157, 159, 211)");
            
            migrationBuilder.Sql("UPDATE \"Counterparties\" " +
                "SET \"ContractKind\" = 2 " +
                "WHERE \"Id\" IN (85, 90, 92, 106, 109, 187)");
        }
    }
}
