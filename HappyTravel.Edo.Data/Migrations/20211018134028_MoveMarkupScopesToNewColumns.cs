using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class MoveMarkupScopesToNewColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies"" SET ""AgentScopeType"" = ""ScopeType"";
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = ""CounterpartyId"" WHERE ""ScopeType"" = 2;
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = ""AgencyId"" WHERE ""ScopeType"" = 3;
                UPDATE ""MarkupPolicies"" SET ""AgentScopeId"" = CONCAT(""AgentId"", '-', ""AgencyId"") WHERE ""ScopeType"" = 4;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies"" SET ""ScopeType"" = ""AgentScopeType"";
                UPDATE ""MarkupPolicies"" SET ""CounterpartyId"" = ""AgentScopeId"" WHERE ""AgentScopeId"" = 2;
                UPDATE ""MarkupPolicies"" SET ""AgencyId"" = ""AgentScopeId"" WHERE ""AgentScopeId"" = 3;
                UPDATE ""MarkupPolicies"" SET ""AgentId"" = SPLIT_PART(""AgentScopeId"", '-', 1), ""AgencyId"" = SPLIT_PART(""AgentScopeId"", '-', 2) WHERE ""AgentScopeId"" = 4;
            ");
        }
    }
}
