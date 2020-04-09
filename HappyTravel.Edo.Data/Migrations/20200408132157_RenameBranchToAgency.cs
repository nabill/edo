using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameBranchToAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Branches
            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "IX_Branches_CounterpartyId",
                newName: "IX_Agencies_CounterpartyId");

            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "PK_Branches",
                newName: "PK_Agencies");

            migrationBuilder.RenameTable(
                name: "Branches",
                newName: "Agencies");

            // CustomerCompanyRelations
            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "BranchId",
                newName: "AgencyId");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "BranchId",
                newName: "AgencyId");

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_BranchId",
                newName: "IX_MarkupPolicies_AgencyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Branches

            migrationBuilder.RenameTable(
                name: "Agencies",
                newName: "Branches");

            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "IX_Agencies_CounterpartyId",
                newName: "IX_Branches_CounterpartyId");

            migrationBuilder.RenameIndex(
                table: "Branches",
                name: "PK_Agencies",
                newName: "PK_Branches");

            // CustomerCompanyRelations
            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "AgencyId",
                newName: "BranchId");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "AgencyId",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_AgencyId",
                newName: "IX_MarkupPolicies_BranchId");
        }
    }
}
