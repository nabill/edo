using HappyTravel.Edo.Common.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CustomerCompanyPermissionsColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var defaultPermissions = CustomerCompanyPermissions.AccommodationBooking |
                CustomerCompanyPermissions.AccommodationAvailabilitySearch;
            
            migrationBuilder.AddColumn<int>(
                name: "Permissions",
                table: "CustomerCompanyRelations",
                nullable: false,
                defaultValue: (int)defaultPermissions);
            
            var defaultMasterCustomerPermissions = CustomerCompanyPermissions.AccommodationBooking |
                CustomerCompanyPermissions.AccommodationAvailabilitySearch |
                CustomerCompanyPermissions.EditCompanyInfo |
                CustomerCompanyPermissions.PermissionManagement |
                CustomerCompanyPermissions.CustomerInvitation;

            migrationBuilder.Sql($"UPDATE \"CustomerCompanyRelations\" SET \"Permissions\"={(int) defaultMasterCustomerPermissions} WHERE \"Type\" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "CustomerCompanyRelations");
        }
    }
}
