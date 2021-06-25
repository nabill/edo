using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAdministratorManagementPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AdministratorRules\" " +
                "SET \"Permissions\" = \"Permissions\" | 131072 " +
                "WHERE \"Id\" = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
