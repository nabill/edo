using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CounterpartyFieldsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "Counterparties");

            migrationBuilder.AddColumn<string>(
                name: "Number",
                table: "Invoices",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Counterparties",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Number = table.Column<string>(nullable: false),
                    ServiceType = table.Column<int>(nullable: false),
                    ServiceSource = table.Column<int>(nullable: false),
                    ParentReferenceCode = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    InvoiceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_InvoiceId",
                table: "Receipts",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ServiceSource_ServiceType_ParentReferenceCode",
                table: "Receipts",
                columns: new[] { "ServiceSource", "ServiceType", "ParentReferenceCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_Number",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Counterparties");

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "Counterparties",
                type: "text",
                nullable: true);
        }
    }
}
