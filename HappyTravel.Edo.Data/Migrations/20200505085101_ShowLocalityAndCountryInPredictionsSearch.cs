using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ShowLocalityAndCountryInPredictionsSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterConcatenatedLocationTsVectorFunction = @"
			CREATE OR REPLACE FUNCTION public.get_location_concatenated_tsvector(name jsonb, locality jsonb, country jsonb)
			RETURNS tsvector
			LANGUAGE plpgsql
			IMMUTABLE
			AS $function$
						DECLARE country_vector tsvector; locality_vector tsvector;	name_vector tsvector;
							 BEGIN 
								country_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(country::json))::jsonb);
								IF locality = '{}' THEN 
									locality_vector = country_vector;
								ELSE 
									locality_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(locality::json))::jsonb);
								END IF;
								IF name = '{}' THEN 
									name_vector = locality_vector;
								ELSE
									name_vector = to_tsvector((SELECT json_agg(regexp_replace(value,'[^\w0-9]+','','g')) FROM json_each_text(name::json))::jsonb);
								END IF;
								RETURN name_vector || locality_vector || country_vector;
								END;
			$function$;";

            migrationBuilder.Sql(alterConcatenatedLocationTsVectorFunction);
            
            var reindexSql = "REINDEX INDEX \"IDX_GetLocationConcatenatedTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
	        
	        var reindexSql = "REINDEX INDEX \"IDX_GetLocationConcatenatedTsVector\"";
	        migrationBuilder.Sql(reindexSql);
        }
    }
}
