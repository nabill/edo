using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameCustomerToAgent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // BookingAuditLog
            migrationBuilder.RenameColumn(
                table: "BookingAuditLog",
                name: "CustomerId",
                newName: "AgentId");

            migrationBuilder.RenameIndex(
                table: "BookingAuditLog",
                name: "IX_BookingAuditLog_CustomerId",
                newName: "IX_BookingAuditLog_AgentId");

            // Bookings
            migrationBuilder.RenameColumn(
                table: "Bookings",
                name: "CustomerId",
                newName: "AgentId");

            migrationBuilder.RenameIndex(
                table: "Bookings",
                name: "IX_Bookings_CustomerId",
                newName: "IX_Bookings_AgentId");

            // CreditCardAuditLogs
            migrationBuilder.RenameColumn(
                table: "CreditCardAuditLogs",
                name: "CustomerId",
                newName: "AgentId");

            // CustomerCompanyRelations
            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "CustomerId",
                newName: "AgentId");

            migrationBuilder.RenameIndex(
                table: "CustomerCounterpartyRelations",
                name: "PK_CustomerCounterpartyRelations",
                newName: "PK_AgentCounterpartyRelations");

            migrationBuilder.RenameTable(
                name: "CustomerCounterpartyRelations",
                newName: "AgentCounterpartyRelations");

            // Customers
            migrationBuilder.RenameIndex(
                table: "Customers",
                name: "PK_Customers",
                newName: "PK_Agents");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Agents");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "CustomerId",
                newName: "AgentId");

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_CustomerId",
                newName: "IX_MarkupPolicies_AgentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // BookingAuditLog
            migrationBuilder.RenameColumn(
                table: "BookingAuditLog",
                name: "AgentId",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                table: "BookingAuditLog",
                name: "IX_BookingAuditLog_AgentId",
                newName: "IX_BookingAuditLog_CustomerId");

            // Bookings
            migrationBuilder.RenameColumn(
                table: "Bookings",
                name: "AgentId",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                table: "Bookings",
                name: "IX_Bookings_AgentId",
                newName: "IX_Bookings_CustomerId");

            // CreditCardAuditLogs
            migrationBuilder.RenameColumn(
                table: "CreditCardAuditLogs",
                name: "AgentId",
                newName: "CustomerId");

            // CustomerCompanyRelations
            migrationBuilder.RenameTable(
                name: "AgentCounterpartyRelations",
                newName: "CustomerCounterpartyRelations");

            migrationBuilder.RenameColumn(
                table: "CustomerCounterpartyRelations",
                name: "AgentId",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                table: "CustomerCounterpartyRelations",
                name: "PK_AgentCounterpartyRelations",
                newName: "PK_CustomerCounterpartyRelations");

            // Customers
            migrationBuilder.RenameIndex(
                table: "Customers",
                name: "PK_Agents",
                newName: "PK_Customers");

            migrationBuilder.RenameTable(
                name: "Agents",
                newName: "Customers");

            // MarkupPolicies
            migrationBuilder.RenameColumn(
                table: "MarkupPolicies",
                name: "AgentId",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                table: "MarkupPolicies",
                name: "IX_MarkupPolicies_AgentId",
                newName: "IX_MarkupPolicies_CustomerId");
        }
    }
}
