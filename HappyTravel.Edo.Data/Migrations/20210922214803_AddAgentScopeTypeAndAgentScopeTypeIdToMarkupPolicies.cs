using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddAgentScopeTypeAndAgentScopeTypeIdToMarkupPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentScopeId",
                table: "MarkupPolicies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentScopeType",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentScopeId",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "AgentScopeType",
                table: "MarkupPolicies");
        }
    }
}
