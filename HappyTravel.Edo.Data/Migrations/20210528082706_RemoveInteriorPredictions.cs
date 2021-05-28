using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveInteriorPredictions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var removeSearchLocationsFunction = "DROP FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer);";
            migrationBuilder.Sql(removeSearchLocationsFunction);
            
            migrationBuilder.DropTable(
                name: "Locations");
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Coordinates = table.Column<Point>(type: "geography (point)", nullable: false),
                    Country = table.Column<string>(type: "jsonb", nullable: false),
                    DefaultCountry = table.Column<string>(type: "text", nullable: false),
                    DefaultLocality = table.Column<string>(type: "text", nullable: false),
                    DefaultName = table.Column<string>(type: "text", nullable: false),
                    DistanceInMeters = table.Column<int>(type: "integer", nullable: false),
                    Locality = table.Column<string>(type: "jsonb", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Suppliers = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });
            
            
            var searchFunctionSql = @"
                        CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) 
                        RETURNS SETOF ""Locations"" LANGUAGE plpgsql AS $function$ 
                        DECLARE parsed_ts_query varchar := plainto_tsquery(search_text) :: varchar; 
                        search_ts_query tsquery;
                        BEGIN 
                        -- If parsed query is empty, we cannot execute fulltext search  
                        IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');
                        RETURN QUERY
                        -- Selects Location data: Name, Country, Locality for combination in one value (Name can be Accommodation Name or LocalityZone name)
                        SELECT loc.*
                        FROM public.""Locations"" as loc
                        WHERE get_location_tsvector(""Name"", ""Locality"", ""Country"") @@ search_ts_query
                        AND ""Type"" = location_type
                        -- This function returns count of consistence with search query ( in concatenated values)
                        ORDER BY get_location_rank(search_text, ""ConcatenatedTsVector"" ) DESC,
                        -- In search result needed Country First than Locality then Name 
                        ""Name"" -> 'en'  NULLS FIRST,
                        ""Locality"" -> 'en'  NULLS FIRST
                        LIMIT (take_limit);
                        ELSE RETURN QUERY
                        SELECT *
                        FROM public.""Locations""
                        LIMIT 0;
                        END IF;
                        END;
                        $function$;";
            
            migrationBuilder.Sql(searchFunctionSql);
        }
    }
}
