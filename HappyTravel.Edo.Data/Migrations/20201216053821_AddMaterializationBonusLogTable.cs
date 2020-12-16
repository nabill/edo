using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddMaterializationBonusLogTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterializationBonusLogs",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "integer", nullable: false),
                    ReferenceCode = table.Column<string>(type: "text", nullable: false),
                    AgencyAccountId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Paid = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterializationBonusLogs", x => new { x.ReferenceCode, x.PolicyId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterializationBonusLogs");
        }
    }
}
