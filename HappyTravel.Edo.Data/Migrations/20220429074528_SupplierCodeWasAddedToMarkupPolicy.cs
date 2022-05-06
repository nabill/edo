using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SupplierCodeWasAddedToMarkupPolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "MarkupPolicies",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Dictionary<string, bool>>(
                name: "EnabledSuppliers",
                table: "AgencySystemSettings",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, bool>),
                oldType: "jsonb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "MarkupPolicies");

            migrationBuilder.AlterColumn<Dictionary<string, bool>>(
                name: "EnabledSuppliers",
                table: "AgencySystemSettings",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, bool>),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
