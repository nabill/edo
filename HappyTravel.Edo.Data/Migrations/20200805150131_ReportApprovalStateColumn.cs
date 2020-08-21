using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ReportApprovalStateColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "AccommodationDuplicateReports");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalState",
                table: "AccommodationDuplicateReports",
                nullable: false,
                defaultValue: 0);

            var addAccommodationsToReportsSql =
                "UPDATE \"AccommodationDuplicateReports\" report\nSET \"Accommodations\" = (\n    SELECT json_agg( json_build_object('DataProvider', (select (json_build_object('Netstorming', 1, 'Illusions', 2, 'Direct', 3, 'Etg', 4)::jsonb)-> split_part((to_jsonb(item) ->> 'AccommodationId1'), '::', 1)), 'Id', split_part((to_jsonb(item) ->> 'AccommodationId1'), '::', 2))) \n    FROM (SELECT \"AccommodationId1\" FROM \"AccommodationDuplicates\" WHERE \"ParentReportId\" = report.\"Id\") item\n)";

            migrationBuilder.Sql(addAccommodationsToReportsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalState",
                table: "AccommodationDuplicateReports");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "AccommodationDuplicateReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
