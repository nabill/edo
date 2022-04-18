using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddEnabledSuppliersToAgencySystemSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, bool>>(
                name: "EnabledSuppliers",
                table: "AgencySystemSettings",
                type: "jsonb",
                nullable: false,
                defaultValue: new Dictionary<string, bool>());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledSuppliers",
                table: "AgencySystemSettings");
        }
    }
}
