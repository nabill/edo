using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameAgentScopeType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AgentScopeType",
                table: "MarkupPolicies",
                newName: "SubjectScopeType");

            migrationBuilder.RenameColumn(
                name: "AgentScopeId",
                table: "MarkupPolicies",
                newName: "SubjectScopeId");

            migrationBuilder.RenameIndex(
                name: "IX_MarkupPolicies_AgentScopeType",
                table: "MarkupPolicies",
                newName: "IX_MarkupPolicies_SubjectScopeType");

            migrationBuilder.RenameIndex(
                name: "IX_MarkupPolicies_AgentScopeId",
                table: "MarkupPolicies",
                newName: "IX_MarkupPolicies_SubjectScopeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubjectScopeType",
                table: "MarkupPolicies",
                newName: "AgentScopeType");

            migrationBuilder.RenameColumn(
                name: "SubjectScopeId",
                table: "MarkupPolicies",
                newName: "AgentScopeId");

            migrationBuilder.RenameIndex(
                name: "IX_MarkupPolicies_SubjectScopeType",
                table: "MarkupPolicies",
                newName: "IX_MarkupPolicies_AgentScopeType");

            migrationBuilder.RenameIndex(
                name: "IX_MarkupPolicies_SubjectScopeId",
                table: "MarkupPolicies",
                newName: "IX_MarkupPolicies_AgentScopeId");
        }
    }
}
