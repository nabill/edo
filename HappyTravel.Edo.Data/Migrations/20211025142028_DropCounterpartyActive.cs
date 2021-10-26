using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class DropCounterpartyActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deactivate agencies with deactivated counterparty
            migrationBuilder.Sql("UPDATE \"Agencies\" a " +
                "SET \"IsActive\" = (SELECT \"IsActive\" FROM \"Counterparties\" c WHERE a.\"CounterpartyId\" = C.\"Id\") " +
                "WHERE a.\"IsActive\" = TRUE");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Counterparties");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Counterparties",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
