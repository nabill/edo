using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCompanyAccountManagementPermissionToFinanceManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CompanyAccountManagement permission to Finance manager
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"Permissions\" = \"Permissions\" | 67108864" +
                " where \"Name\" = 'Finance manager';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
