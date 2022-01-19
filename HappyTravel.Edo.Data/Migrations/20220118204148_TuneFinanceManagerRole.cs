using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class TuneFinanceManagerRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Added booking reports generation permission
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Permissions\" = \"Permissions\" | 16384 " +
                "where \"Name\" = 'Finance manager'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
