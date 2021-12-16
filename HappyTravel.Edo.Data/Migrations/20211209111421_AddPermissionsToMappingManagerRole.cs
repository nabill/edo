using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddPermissionsToMappingManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""AdministratorRoles"" 
                SET ""Permissions"" = 787456
                WHERE ""Id"" = 6
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
