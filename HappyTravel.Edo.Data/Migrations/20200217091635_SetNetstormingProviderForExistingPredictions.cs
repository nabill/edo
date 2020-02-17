using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SetNetstormingProviderForExistingPredictions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var setPredictionsSql = "UPDATE \"Locations\" SET \"DataProviders\" = '[1]'::jsonb WHERE \"DataProviders\" = '[]'::jsonb;";
            migrationBuilder.Sql(setPredictionsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
