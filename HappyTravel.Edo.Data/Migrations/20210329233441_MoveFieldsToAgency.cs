using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveFieldsToAgency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Agencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Agencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Agencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Agencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredCurrency",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VatNumber",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Agencies",
                type: "text",
                nullable: true);


            migrationBuilder.Sql(
                "UPDATE \"Agencies\" a " +
                "SET \"Address\" = (SELECT \"Address\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"BillingEmail\" = (SELECT \"BillingEmail\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"City\" = (SELECT \"City\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"CountryCode\" = (SELECT \"CountryCode\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"Fax\" = (SELECT \"Fax\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"Phone\" = (SELECT \"Phone\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"PostalCode\" = (SELECT \"PostalCode\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"PreferredCurrency\" = (SELECT \"PreferredCurrency\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"VatNumber\" = (SELECT \"VatNumber\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") " +
                "SET \"Website\" = (SELECT \"Website\" FROM \"Counterparties\" WHERE \"Id\" = a.\"CounterpartyId\") ");

            
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
                name: "PreferredCurrency",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Counterparties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "PreferredCurrency",
                table: "Counterparties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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


            migrationBuilder.Sql(
                "UPDATE \"Counterparties\" c " +
                "SET \"Address\" = (SELECT \"Address\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"BillingEmail\" = (SELECT \"BillingEmail\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"City\" = (SELECT \"City\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"CountryCode\" = (SELECT \"CountryCode\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"Fax\" = (SELECT \"Fax\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"Phone\" = (SELECT \"Phone\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"PostalCode\" = (SELECT \"PostalCode\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"PreferredCurrency\" = (SELECT \"PreferredCurrency\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"VatNumber\" = (SELECT \"VatNumber\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) " +
                "SET \"Website\" = (SELECT \"Website\" FROM \"Agencies\" a WHERE a.\"CounterpartyId\" = c.\"Id\" AND a.\"ParentId\" IS NULL) ");


            migrationBuilder.DropColumn(
                name: "Address",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "PreferredCurrency",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "VatNumber",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Agencies");
        }
    }
}
