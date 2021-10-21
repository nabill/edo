using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ReorderAgentMarkupScopeTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 0 
                WHERE ""AgentMarkupScopeType"" = 5
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 6 
                WHERE ""AgentMarkupScopeType"" = 4
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 5 
                WHERE ""AgentMarkupScopeType"" = 3
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 4 
                WHERE ""AgentMarkupScopeType"" = 2
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 2 
                WHERE ""AgentMarkupScopeType"" = 4
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 3 
                WHERE ""AgentMarkupScopeType"" = 5
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""MarkupPolicies""
                SET ""AgentMarkupScopeType"" = 4 
                WHERE ""AgentMarkupScopeType"" = 6
            ");
        }
    }
}
