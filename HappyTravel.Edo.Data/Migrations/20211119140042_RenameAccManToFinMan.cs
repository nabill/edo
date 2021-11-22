using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameAccManToFinMan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Name\" = 'Finance manager' " +
                "where \"Id\" = 2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
