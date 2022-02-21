using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdatePoliciesWithTemplateData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var fillFactorSql = "update \"MarkupPolicies\" set \"Value\" = (\"TemplateSettings\"->'factor')::decimal * 100 - 100,  \"FunctionType\" = 1 where \"TemplateSettings\"->'factor' is not null;";
            migrationBuilder.Sql(fillFactorSql);

            var fillAdditionSql =
                "update \"MarkupPolicies\" set \"Value\" = (\"TemplateSettings\"->'addition')::decimal, \"FunctionType\" = 2 where \"TemplateSettings\"->'addition' is not null;";

            migrationBuilder.Sql(fillAdditionSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
