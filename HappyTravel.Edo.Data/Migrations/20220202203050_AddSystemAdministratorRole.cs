using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddSystemAdministratorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            @migrationBuilder.Sql(@"
            INSERT INTO ""AdministratorRoles""
                (""Name"", ""Permissions"", ""NotificationTypes"")
            VALUES('System Administrator', 4194304 | 8388608, null);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
