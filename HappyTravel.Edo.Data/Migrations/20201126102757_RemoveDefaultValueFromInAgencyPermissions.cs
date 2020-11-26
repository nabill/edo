using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveDefaultValueFromInAgencyPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = \"InAgencyPermissions\" >> 1 WHERE \"InAgencyPermissions\" != 2147483646;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = \"InAgencyPermissions\" << 1 WHERE \"InAgencyPermissions\" != 2147483646;");
        }
    }
}
