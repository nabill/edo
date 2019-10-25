using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FuncAndIdxForLocationSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION get_location_tsvector(country jsonb, locality jsonb, name jsonb)
                                    RETURNS tsvector AS $$
                                    BEGIN
                                        RETURN setweight(to_tsvector(country), 'A') || ' ' || setweight(to_tsvector(locality), 'B')  || ' ' || setweight(to_tsvector(name), 'C');
                                    END
                                    $$ LANGUAGE 'plpgsql' IMMUTABLE;");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IDX_GetLocationTsVector\" ON \"Locations\" USING gin(get_location_tsvector(\"Country\", \"Locality\", \"Name\"));");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION get_location_tsvector;");
            migrationBuilder.Sql("DROP INDEX \"IDX_GetLocationTsVector\";");
        }
    }
}
