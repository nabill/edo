using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAgencyMarkupBonusesAccountsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyMarkupBonusesAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgencyId = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyMarkupBonusesAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyMarkupBonusesAccounts_AgencyId",
                table: "AgencyMarkupBonusesAccounts",
                column: "AgencyId");
            
            
            migrationBuilder.Sql(@"
                INSERT INTO ""AgencyMarkupBonusesAccounts""(""AgencyId"", ""Currency"", ""Balance"")
                SELECT ""Id"", 1, 0
                FROM ""Agencies""
                WHERE ""IsActive"" IS true
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyMarkupBonusesAccounts");
        }
    }
}
