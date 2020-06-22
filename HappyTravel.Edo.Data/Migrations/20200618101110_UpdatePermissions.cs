using HappyTravel.Edo.Common.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdatePermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var masterPermission = InAgencyPermissions.PermissionManagement | InAgencyPermissions.AccommodationAvailabilitySearch;
            migrationBuilder.Sql(
                $"UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\"={(int)masterPermission} WHERE \"Type\" = 1"
            );

            var regularPermission = InAgencyPermissions.AccommodationAvailabilitySearch;
            migrationBuilder.Sql(
                $"UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\"={(int)regularPermission} WHERE \"Type\" = 0"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
