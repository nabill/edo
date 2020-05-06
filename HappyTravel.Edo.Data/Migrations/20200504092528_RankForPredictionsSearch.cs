using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RankForPredictionsSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterLocationRankFunction = @"
            CREATE OR REPLACE FUNCTION public.get_location_rank(searchtext text, locationtsvector tsvector)
            RETURNS int
            LANGUAGE plpgsql
            AS $function$
	            DECLARE location_rank int;
	            BEGIN
            SELECT count(*) INTO location_rank FROM regexp_split_to_table(searchtext, '[^a-z0-9]') st
            WHERE st<> '' AND locationtsvector @@ to_tsquery(st || ':*');
            RETURN location_rank;
	            END;
            $function$;";

            migrationBuilder.Sql(alterLocationRankFunction);

            var alterConcatenatedLocationTsVectorFunction = @"
			CREATE OR REPLACE FUNCTION public.get_location_concatenated_tsvector(name jsonb, locality jsonb, country jsonb)
				RETURNS tsvector
				LANGUAGE plpgsql
			IMMUTABLE
			AS $function$
			DECLARE country_vector tsvector; locality_vector tsvector;	name_vector tsvector;
							 BEGIN 
								country_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(country::json))::jsonb);
								locality_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(locality::json))::jsonb);
								IF locality_vector = '' THEN locality_vector = country_vector;
								END IF;
								name_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(name::json))::jsonb);
								IF name_vector = '' THEN name_vector = locality_vector;
								END IF;
								RETURN name_vector || locality_vector || country_vector;
								END
			$function$;";

            migrationBuilder.Sql(alterConcatenatedLocationTsVectorFunction);

            var createIndexGetLocationConcatenatedTsVector =
                "CREATE INDEX IF NOT EXISTS \"IDX_GetLocationConcatenatedTsVector\" ON \"Locations\" USING gin(get_location_concatenated_tsvector(\"Locality\", \"Name\", \"Country\"))";
            migrationBuilder.Sql(createIndexGetLocationConcatenatedTsVector);

            var alterLocationTsVectorFunction = @"
			CREATE OR REPLACE FUNCTION public.get_location_tsvector(name jsonb, locality jsonb, country jsonb)
			RETURNS tsvector
			LANGUAGE plpgsql
			IMMUTABLE
			AS $function$ DECLARE country_vector tsvector; locality_vector tsvector; name_vector tsvector;
							 BEGIN country_vector =  to_tsvector(country);
							 locality_vector = to_tsvector(locality);
							 IF locality_vector = '' THEN locality_vector = country_vector;
							 END IF;
							 name_vector = to_tsvector(name);
							 IF name_vector = '' THEN name_vector = locality_vector;
							 END IF;
							 RETURN setweight(name_vector, 'A') || setweight(locality_vector,'B') || setweight(country_vector,'C');
							 END $function$;";

            migrationBuilder.Sql(alterLocationTsVectorFunction);

            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);

            var alterSearchLocationsFunction =
                "CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) \n RETURNS SETOF \"Locations\" LANGUAGE plpgsql AS $function$ \n DECLARE parsed_ts_query varchar := plainto_tsquery(search_text) :: varchar; \n search_ts_query tsquery;\n BEGIN -- If parsed query is empty, we cannot execute fulltext search  \n IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');\n RETURN QUERY\n SELECT loc.*\n FROM public.\"Locations\" as loc\n WHERE get_location_tsvector(\"Name\", \"Locality\", \"Country\") @@ search_ts_query\n   AND \"Type\" = location_type\n ORDER BY get_location_rank(search_text, get_location_concatenated_tsvector(\"Name\", \"Locality\", \"Country\")) DESC,\nts_rank( get_location_tsvector(\"Name\", \"Locality\", \"Country\"), search_ts_query) DESC\n LIMIT (take_limit);\n ELSE RETURN QUERY\n SELECT *\n FROM public.\"Locations\"\n LIMIT 0;\n END IF;\n END;\n $function$;";
            migrationBuilder.Sql(alterSearchLocationsFunction);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX \"IDX_GetLocationConcatenatedTsVector\";");
            migrationBuilder.Sql("DROP FUNCTION get_location_concatenated_tsvector;");
            migrationBuilder.Sql("DROP FUNCTION get_location_rank;");

            var alterSearchLocationsFunction =
                "CREATE OR REPLACE FUNCTION public.search_locations(search_text text, location_type integer, take_limit integer DEFAULT 10) \n RETURNS SETOF \"Locations\" LANGUAGE plpgsql AS $function$ \n DECLARE parsed_ts_query varchar := plainto_tsquery(search_text) :: varchar; \n search_ts_query tsquery;\n BEGIN -- If parsed query is empty, we cannot execute fulltext search  \n IF parsed_ts_query <> '' THEN search_ts_query := to_tsquery(parsed_ts_query || ':*');\n RETURN QUERY\n SELECT loc.*\n FROM public.\"Locations\" as loc\n WHERE get_location_tsvector(\"Name\", \"Locality\", \"Country\") @@ search_ts_query\n   AND \"Type\" = location_type\n ORDER BY ts_rank( get_location_tsvector(\"Name\", \"Locality\", \"Country\"), search_ts_query) DESC\n LIMIT (take_limit);\n ELSE RETURN QUERY\n SELECT *\n FROM public.\"Locations\"\n LIMIT 0;\n END IF;\n END;\n $function$;";
            migrationBuilder.Sql(alterSearchLocationsFunction);

            var alterGetLocationTsVectorFunction = @"CREATE
                    OR REPLACE FUNCTION public.get_location_tsvector(name jsonb, locality jsonb, country jsonb) RETURNS tsvector LANGUAGE plpgsql IMMUTABLE AS $function$ DECLARE country_vector tsvector;
                    locality_vector tsvector;
                    name_vector tsvector;
                    BEGIN country_vector = to_tsvector('simple', replace(country :: text, ' ', '') :: jsonb);
                    locality_vector = to_tsvector('simple', replace(locality :: text, ' ', '') :: jsonb);
                    IF locality_vector = '' THEN locality_vector = country_vector;
                    END IF;
                    name_vector = to_tsvector('simple', replace(name :: text, ' ', '') :: jsonb);
                    IF name_vector = '' THEN name_vector = locality_vector;
                    END IF;
                    RETURN setweight(name_vector, 'A') || setweight(locality_vector, 'B') || setweight(country_vector, 'C');
                    END $function$;";

            migrationBuilder.Sql(alterGetLocationTsVectorFunction);

            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }
    }
}