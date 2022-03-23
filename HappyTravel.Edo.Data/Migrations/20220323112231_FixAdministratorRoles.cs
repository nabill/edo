using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixAdministratorRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from \"AdministratorRoles\" where \"Name\" = 'Reports manager';");
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"Name\" = 'Report manager' where \"Name\" = 'Auditor';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
