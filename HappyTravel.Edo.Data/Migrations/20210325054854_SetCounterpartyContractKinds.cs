using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SetCounterpartyContractKinds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "-- Cash\n" +
                "UPDATE \"Counterparties\"\n" +
                "SET \"ContractKind\" = 1\n" +
                "WHERE \"Id\" in (76, 86, 88, 89, 91, 93, 94, 102, 104, 105, 110, 111, 113, 115, 117, 118, 119, 120, 121, 124, 127, 131, 132, 133, 134, 136, 137, 142, 143, 145, 147, 148, 150, 152, 153, 156, 157, 159, 167, 191, 193, 195, 211, 212, 213, 214, 234, 236, 237, 239, 240, 241, 242, 244, 245, 246, 247, 249, 251, 264, 267, 268);\n" +
                "\n" +
                
                "-- Virtual credit\n" +
                "UPDATE \"Counterparties\"\n" +
                "SET \"ContractKind\" = 2\n" +
                "WHERE \"Id\" in (85, 90, 92, 95, 106, 109, 162, 168, 169, 170, 172, 173, 174, 176, 183, 184, 185, 187, 190, 192, 194, 196, 210, 243);\n\n" +
                "\n" +
                
                "-- Credit card\n" +
                "UPDATE \"Counterparties\"\n" +
                "SET \"ContractKind\" = 3\n" +
                "WHERE \"Id\" in (87, 107, 116, 125, 130, 151, 186, 188, 197, 215, 216, 219, 229, 238, 248, 262, 266, 272);";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
