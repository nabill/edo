using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveInAgencyPermissionsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InAgencyPermissions",
                table: "AgentAgencyRelations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InAgencyPermissions",
                table: "AgentAgencyRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
