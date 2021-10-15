using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SetAdministratorPreserved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentRoles\" " +
                "SET \"IsPreservedInAgency\" = TRUE " +
                "WHERE \"Name\" = 'Agency administrator' ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
