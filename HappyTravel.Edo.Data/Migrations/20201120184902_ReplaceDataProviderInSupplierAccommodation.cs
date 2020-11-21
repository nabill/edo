using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ReplaceDataProviderInSupplierAccommodation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var changeAttributeSql =
                "update \"AccommodationDuplicateReports\" \nset \"Accommodations\" = replace(\"Accommodations\"::text, '\"DataProvider\"', '\"Supplier\"')::jsonb";

            migrationBuilder.Sql(changeAttributeSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var changeAttributeSql =
                "update \"AccommodationDuplicateReports\" \nset \"Accommodations\" = replace(\"Accommodations\"::text, '\"Supplier\"', '\"DataProvider\"')::jsonb";

            migrationBuilder.Sql(changeAttributeSql);
        }
    }
}
