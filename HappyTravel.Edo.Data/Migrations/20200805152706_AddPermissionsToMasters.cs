using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPermissionsToMasters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var updatePermissionsForMaster = "UPDATE \"AgentAgencyRelations\" \nSET \"InAgencyPermissions\" = 1022\nWHERE \"Type\" = 1\n";
            migrationBuilder.Sql(updatePermissionsForMaster);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
