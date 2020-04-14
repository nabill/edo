using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameCompanyToCounterparty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Companies
            migrationBuilder.RenameIndex(
                table: "Companies",
                name: "PK_Companies",
                newName: "PK_Counterparties");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Counterparties");

            // CustomerCompanyRelations
            migrationBuilder.RenameColumn(
                table: "CustomerCompanyRelations",
                name: "CompanyId",
                newName: "CounterpartyId"
            );

            migrationBuilder.RenameColumn(
                table: "CustomerCompanyRelations",
                name: "InCompanyPermissions",
                newName: "InCounterpartyPermissions"
            );

            migrationBuilder.RenameIndex(
                table: "CustomerCompanyRelations",
                name: "PK_CustomerCompanyRelations",
                newName: "PK_CustomerCounterpartyRelations");

            migrationBuilder.RenameTable(
                name: "CustomerCompanyRelations",
                newName: "CustomerCounterpartyRelations");

            // Bookings
            migrationBuilder.RenameColumn(
                table: "Bookings",
                name: "CompanyId",
                newName: "CounterpartyId"
            );

            migrationBuilder.RenameIndex(
                table: "Bookings",
                name: "IX_Bookings_CompanyId",
                newName: "IX_Bookings_CounterpartyId");

            // Branches
            migrationBuilder.RenameColumn(
                table: "Branches",
                name: "CompanyId",
                newName: "CounterpartyId"
            );

            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "IX_Branches_CompanyId",
                newName: "IX_Branches_CounterpartyId");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "CompanyId",
                newName: "CounterpartyId"
            );

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_CompanyId",
                newName: "IX_MarkupPolicies_CounterpartyId");
            
            // PaymentAccounts
            migrationBuilder.RenameColumn(
                table: "PaymentAccounts",
                name: "CompanyId",
                newName: "CounterpartyId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Companies
            migrationBuilder.RenameIndex(
                table: "Counterparties",
                name: "PK_Counterparties",
                newName: "PK_Companies");

            migrationBuilder.RenameTable(
                name: "Counterparties",
                newName: "Companies");

            // CustomerCompanyRelations
            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "CounterpartyId",
                newName: "CompanyId"
            );

            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "InCounterpartyPermissions",
                newName: "InCompanyPermissions"
            );

            migrationBuilder.RenameIndex(
                table: "CustomerCounterpartyRelations",
                name: "PK_CustomerCounterpartyRelations",
                newName: "PK_CustomerCompanyRelations");

            migrationBuilder.RenameTable(
                name: "CustomerCounterpartyRelations",
                newName: "CustomerCompanyRelations");

            // Bookings
            migrationBuilder.RenameColumn(
                table: "Bookings",
                name: "CounterpartyId",
                newName: "CompanyId"
            );

            migrationBuilder.RenameIndex(
                table: "Bookings",
                name: "IX_Bookings_CounterpartyId",
                newName: "IX_Bookings_CompanyId");

            // Branches
            migrationBuilder.RenameColumn(
                table: "Branches",
                name: "CounterpartyId",
                newName: "CompanyId"
            );

            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "IX_Branches_CounterpartyId",
                newName: "IX_Branches_CompanyId");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "CounterpartyId",
                newName: "CompanyId"
            );

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_CounterpartyId",
                newName: "IX_MarkupPolicies_CompanyId");

            // PaymentAccounts
            migrationBuilder.RenameColumn(
                table: "PaymentAccounts",
                name: "CounterpartyId",
                newName: "CompanyId"
            );
        }
    }
}
