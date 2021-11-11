using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveCounterpartyPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("" +
                "update \"MarkupPolicies\" " +
                "set \"SubjectScopeType\" = 5, " +
                "\"SubjectScopeId\" = (select \"Id\" from \"Agencies\" where \"CounterpartyId\" = \"SubjectScopeId\"::integer and \"ParentId\" is null)::text " +
                "where \"SubjectScopeType\" = 4 ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
