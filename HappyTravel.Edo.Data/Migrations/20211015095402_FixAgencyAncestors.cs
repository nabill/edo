using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixAgencyAncestors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Agencies""
                SET ""Ancestors"" = '{}'
                WHERE ""Ancestors"" IS null
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Agencies""
                SET ""Ancestors"" = array_append(""Ancestors"", ""ParentId"")
                WHERE ""ParentId"" IS not null AND ""Ancestors"" = '{}'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
