using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameAddressLegalAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Counterparties",
                newName: "LegalAddress");

            migrationBuilder.RenameColumn(
                name: "LegalAddress",
                table: "Agencies",
                newName: "Address");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LegalAddress",
                table: "Counterparties",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Agencies",
                newName: "LegalAddress");
        }
    }
}
