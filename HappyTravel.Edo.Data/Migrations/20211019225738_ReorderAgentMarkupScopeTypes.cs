using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ReorderAgentMarkupScopeTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 0 
                WHERE ""AgentScopeType"" = 5
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 6 
                WHERE ""AgentScopeType"" = 4
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 5 
                WHERE ""AgentScopeType"" = 3
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 4 
                WHERE ""AgentScopeType"" = 2
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 2 
                WHERE ""AgentScopeType"" = 4
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 3 
                WHERE ""AgentScopeType"" = 5
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentScopeType"" = 4 
                WHERE ""AgentScopeType"" = 6
            ");
        }
    }
}
