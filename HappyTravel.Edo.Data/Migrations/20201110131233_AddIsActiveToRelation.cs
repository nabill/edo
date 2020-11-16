using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddIsActiveToRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AgentAgencyRelations",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 8190 WHERE \"InAgencyPermissions\" = 4094;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AgentAgencyRelations");

            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4094 WHERE \"InAgencyPermissions\" = 8190;");
        }
    }
}
