using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ModifyCounterpartiesAddContacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Counterparties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"Counterparties\" c " +
                "SET \"Address\" = a.\"Address\", \"BillingEmail\" = a.\"BillingEmail\", \"City\" = a.\"City\", " +
                    "\"CountryCode\" = a.\"CountryCode\", \"Fax\" = a.\"Fax\", \"Phone\" = a.\"Phone\", " +
                    "\"PostalCode\" = a.\"PostalCode\", \"Website\" = a.\"Website\", \"VatNumber\" = a.\"VatNumber\" " +
                "FROM \"Agencies\" a " +
                "WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" = NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Counterparties");
        }
    }
}
