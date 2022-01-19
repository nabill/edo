using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class TuneAccountManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Permissions\" = \"Permissions\" | 32 " +
                "where \"Name\" = 'Account manager'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
