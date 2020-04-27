using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ConsiderWordsInPredictionsSearchQueryAsSeparate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterSearchLocationsFunction = "CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) \n RETURNS SETOF \"Locations\" LANGUAGE plpgsql AS $function$ \n DECLARE parsed_ts_query varchar := plainto_tsquery(search_text) :: varchar; \n search_ts_query tsquery;\n BEGIN -- If parsed query is empty, we cannot execute fulltext search  \n IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');\n RETURN QUERY\n SELECT loc.*\n FROM public.\"Locations\" as loc\n WHERE get_location_tsvector(\"Name\", \"Locality\", \"Country\") @@ search_ts_query\n   AND \"Type\" = location_type\n ORDER BY ts_rank( get_location_tsvector(\"Name\", \"Locality\", \"Country\"), search_ts_query) DESC\n LIMIT (take_limit);\n ELSE RETURN QUERY\n SELECT *\n FROM public.\"Locations\"\n LIMIT 0;\n END IF;\n END;\n $function$;";
            migrationBuilder.Sql(alterSearchLocationsFunction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var alterSearchLocationsFunction = "CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) \n RETURNS SETOF \"Locations\" LANGUAGE plpgsql AS $function$ \n DECLARE parsed_ts_query varchar := plainto_tsquery(replace(search_text, ' ', '')) :: varchar; \n search_ts_query tsquery;\n BEGIN -- If parsed query is empty, we cannot execute fulltext search  \n IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');\n RETURN QUERY\n SELECT loc.*\n FROM public.\"Locations\" as loc\n WHERE get_location_tsvector(\"Name\", \"Locality\", \"Country\") @@ search_ts_query\n   AND \"Type\" = location_type\n ORDER BY ts_rank( get_location_tsvector(\"Name\", \"Locality\", \"Country\"), search_ts_query) DESC\n LIMIT (take_limit);\n ELSE RETURN QUERY\n SELECT *\n FROM public.\"Locations\"\n LIMIT 0;\n END IF;\n END;\n $function$;";
            migrationBuilder.Sql(alterSearchLocationsFunction);
        }
    }
}
