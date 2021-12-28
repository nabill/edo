using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SeparatePermisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // remove permissions "AgencyManagement" 256, "AgentManagement" 4096 from role "Finance manager"
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Permissions\" = \"Permissions\" & ~256 & ~4096 " +
                "where \"Id\" in (2)");

            // add permissions "ViewAgencies" 1048576, "ViewAgents" 2097152 to roles "Account manager" and "Finance manager"
            migrationBuilder.Sql("update \"AdministratorRoles\" " +
                "set \"Permissions\" = \"Permissions\" | 1048576 | 2097152 " +
                "where \"Id\" in (1, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
