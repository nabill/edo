using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenamePaymentAccountToAgencyAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "PaymentAccounts",
                newName: "AgencyAccounts");

            migrationBuilder.RenameIndex(
                name: "PK_PaymentAccounts",
                newName: "PK_AgencyAccounts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AgencyAccounts",
                newName: "PaymentAccounts");

            migrationBuilder.RenameIndex(
                name: "PK_AgencyAccounts",
                newName: "PK_PaymentAccounts");
        }
    }
}
