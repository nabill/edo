using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdateAgentsAllPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4094 WHERE \"InAgencyPermissions\" =  2046");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 2046 WHERE \"InAgencyPermissions\" =  4094");
        }
    }
}
