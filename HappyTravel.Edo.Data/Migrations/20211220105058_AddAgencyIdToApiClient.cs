using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAgencyIdToApiClient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentDirectApiClientRelations",
                table: "AgentDirectApiClientRelations");

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "AgentDirectApiClientRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update \"AgentDirectApiClientRelations\" cli " +
                "set \"AgencyId\" = (select \"AgencyId\" from \"AgentAgencyRelations\" rel where rel.\"AgentId\" = cli.\"AgentId\")");

            migrationBuilder.AlterColumn<int>(
                table: "AgentDirectApiClientRelations",
                name: "AgencyId",
                defaultValue: null);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentDirectApiClientRelations",
                table: "AgentDirectApiClientRelations",
                columns: new[] { "AgentId", "AgencyId", "DirectApiClientId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentDirectApiClientRelations",
                table: "AgentDirectApiClientRelations");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "AgentDirectApiClientRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentDirectApiClientRelations",
                table: "AgentDirectApiClientRelations",
                columns: new[] { "AgentId", "DirectApiClientId" });
        }
    }
}
