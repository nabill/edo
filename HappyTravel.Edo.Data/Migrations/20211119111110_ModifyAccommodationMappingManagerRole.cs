using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ModifyAccommodationMappingManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""AdministratorRoles"" 
                SET ""Permissions"" = 132096
                WHERE ""Id"" = 6
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
