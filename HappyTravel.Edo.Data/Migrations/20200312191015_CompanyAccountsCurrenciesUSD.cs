using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CompanyAccountsCurrenciesUSD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var migrateToUsd = $"update \"PaymentAccounts\" set \"Currency\" = {(int)Currencies.USD}";
            migrationBuilder.Sql(migrateToUsd);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
