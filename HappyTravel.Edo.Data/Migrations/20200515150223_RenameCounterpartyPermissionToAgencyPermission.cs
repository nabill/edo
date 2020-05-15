using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameCounterpartyPermissionToAgencyPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                table: "AgentCounterpartyRelations",
                name: "InCounterpartyPermissions",
                newName: "InAgencyPermissions");

            migrationBuilder.RenameTable(
                name: "AgentCounterpartyRelations",
                newName: "AgentAgencyRelations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "AgentAgencyRelations",
                newName: "AgentCounterpartyRelations");

            migrationBuilder.RenameColumn(
                table: "AgentCounterpartyRelations",
                newName: "InAgencyPermissions",
                name: "InCounterpartyPermissions");
        }
    }
}
