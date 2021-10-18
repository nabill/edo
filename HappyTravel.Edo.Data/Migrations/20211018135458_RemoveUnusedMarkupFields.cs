using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveUnusedMarkupFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_AgencyId",
                table: "MarkupPolicies");

            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_AgentId",
                table: "MarkupPolicies");

            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_CounterpartyId",
                table: "MarkupPolicies");

            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_ScopeType",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "CounterpartyId",
                table: "MarkupPolicies");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "MarkupPolicies");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_AgentScopeId",
                table: "MarkupPolicies",
                column: "AgentScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_AgentScopeType",
                table: "MarkupPolicies",
                column: "AgentScopeType");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_DestinationScopeId",
                table: "MarkupPolicies",
                column: "DestinationScopeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_AgentScopeId",
                table: "MarkupPolicies");

            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_AgentScopeType",
                table: "MarkupPolicies");

            migrationBuilder.DropIndex(
                name: "IX_MarkupPolicies_DestinationScopeId",
                table: "MarkupPolicies");

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "MarkupPolicies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgentId",
                table: "MarkupPolicies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterpartyId",
                table: "MarkupPolicies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeType",
                table: "MarkupPolicies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_AgencyId",
                table: "MarkupPolicies",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_AgentId",
                table: "MarkupPolicies",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_CounterpartyId",
                table: "MarkupPolicies",
                column: "CounterpartyId");

            migrationBuilder.CreateIndex(
                name: "IX_MarkupPolicies_ScopeType",
                table: "MarkupPolicies",
                column: "ScopeType");
        }
    }
}
