using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class DropCounterpartyVerification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractKind",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "VerificationReason",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Counterparties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractKind",
                table: "Counterparties",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Counterparties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VerificationReason",
                table: "Counterparties",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Verified",
                table: "Counterparties",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
