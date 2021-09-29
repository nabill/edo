using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillAgentScopeIdInMarkupPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies"" SET ""AgentScopeType"" = ""ScopeType"";
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = ""CounterpartyId"" WHERE ""ScopeType"" = 2;
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = ""AgencyId"" WHERE ""ScopeType"" = 3;
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = CONCAT(""AgencyId"", '-', ""AgentId"") WHERE ""ScopeType"" = 4;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = null");
            migrationBuilder.Sql(@"UPDATE ""MarkupPolicies"" SET ""AgentScopeType"" = 0");
        }
    }
}
