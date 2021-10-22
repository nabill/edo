using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddVerificationAgencyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractKind",
                table: "Agencies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationReason",
                table: "Agencies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationState",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Verified",
                table: "Agencies",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractKind",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "VerificationReason",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "VerificationState",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Agencies");
        }
    }
}
