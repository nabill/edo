using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CleanMarkupPolicies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""MarkupPolicies"" 
                WHERE 
                      (""SubjectScopeType"" = 5 OR
                      ""SubjectScopeType"" = 6) AND
                      (""DestinationScopeType"" > 0 OR
                       ""FunctionType"" > 1)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
