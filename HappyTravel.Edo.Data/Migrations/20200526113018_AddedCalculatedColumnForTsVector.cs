using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddedCalculatedColumnForTsVector : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create generated Column for ConcatenatedTsVector
            migrationBuilder.Sql(
                @" ALTER TABLE ""Locations"" 
                    ADD COLUMN ""ConcatenatedTsVector"" tsvector GENERATED ALWAYS AS (get_location_concatenated_tsvector(""Name"",""Locality"",""Country"")) STORED");
            
            migrationBuilder.Sql("DROP INDEX \"IDX_GetLocationConcatenatedTsVector\";");

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


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Locations""
                DROP COLUMN  ""ConcatenatedTsVector""");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IDX_GetLocationConcatenatedTsVector\" ON \"Locations\" USING gin(get_location_concatenated_tsvector(\"Locality\", \"Name\", \"Country\"))");

            var alterSearchLocationsFunction =
                "CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) \n RETURNS SETOF \"Locations\" LANGUAGE plpgsql AS $function$ \n DECLARE parsed_ts_query varchar := plainto_tsquery(search_text) :: varchar; \n search_ts_query tsquery;\n BEGIN -- If parsed query is empty, we cannot execute fulltext search  \n IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');\n RETURN QUERY\n SELECT loc.*\n FROM public.\"Locations\" as loc\n WHERE get_location_tsvector(\"Name\", \"Locality\", \"Country\") @@ search_ts_query\n   AND \"Type\" = location_type\n ORDER BY get_location_rank(search_text, get_location_concatenated_tsvector(\"Name\", \"Locality\", \"Country\")) DESC,\nts_rank( get_location_tsvector(\"Name\", \"Locality\", \"Country\"), search_ts_query) DESC\n LIMIT (take_limit);\n ELSE RETURN QUERY\n SELECT *\n FROM public.\"Locations\"\n LIMIT 0;\n END IF;\n END;\n $function$;";
            migrationBuilder.Sql(alterSearchLocationsFunction);
        }
    }
}