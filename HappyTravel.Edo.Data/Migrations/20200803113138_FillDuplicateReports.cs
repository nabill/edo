using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillDuplicateReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var fillDuplicateReportsSql =
                "insert into \"AccommodationDuplicateReports\" (\"Created\", \"Modified\", \"ReporterAgentId\", \"ReporterAgencyId\", \"IsApproved\")\n\nselect split_part(identifier, '==', 1)::timestamp as created,\n       split_part(identifier, '==', 1)::timestamp as modified,\n       split_part(identifier, '==', 2)::integer as reporter_agent_id,\n       split_part(identifier, '==', 3)::integer as reporter_agency_id,\n       false\n       \nfrom \n(select  distinct(concat(\"Created\", '==', \"ReporterAgentId\", '==', \"ReporterAgencyId\")) as identifier from \"AccommodationDuplicates\")\nas distinct_identifiers;\n\n\nupdate \"AccommodationDuplicates\"\nset \"ParentReportId\" = (select \"Id\" from \"AccommodationDuplicateReports\" where \"AccommodationDuplicates\".\"Created\" = \"AccommodationDuplicateReports\".\"Created\")";
            migrationBuilder.Sql(fillDuplicateReportsSql);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
