using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveCounterpartyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegalAddress",
                table: "Agencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreferredPaymentMethod",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"Agencies\" a " +
                "SET \"LegalAddress\" = (SELECT C.\"LegalAddress\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\"), " +
                "\"PreferredPaymentMethod\" = (SELECT C.\"PreferredPaymentMethod\" FROM \"Counterparties\" c WHERE C.\"Id\" = a.\"CounterpartyId\") " +
                "WHERE a.\"ParentId\" IS null");

            migrationBuilder.DropColumn(
                name: "LegalAddress",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "Counterparties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegalAddress",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreferredPaymentMethod",
                table: "Counterparties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"Counterparties\" C " +
                "SET \"LegalAddress\" = (SELECT a.\"LegalAddress\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = C.\"Id\" AND a.\"ParentId\" IS NULL), " +
                "\"PreferredPaymentMethod\" = (SELECT a.\"PreferredPaymentMethod\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = C.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "WHERE EXISTS (SELECT 1 FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = C.\"Id\" AND a.\"ParentId\" IS NULL)");

            migrationBuilder.DropColumn(
                name: "LegalAddress",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "PreferredPaymentMethod",
                table: "Agencies");
        }
    }
}
