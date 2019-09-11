using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CompanyVerifiedColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Verified",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifyReason",
                table: "Companies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Verified",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "VerifyReason",
                table: "Companies");
        }
    }
}
