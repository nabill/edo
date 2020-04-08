using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class PredictionsSearchChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterGetLocationsTsVector = @"CREATE
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

            migrationBuilder.Sql(alterGetLocationsTsVector);

            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var alterGetLocationsTsVector = @"CREATE OR REPLACE FUNCTION public.get_location_tsvector(name jsonb, locality jsonb, country jsonb)
            RETURNS tsvector
            LANGUAGE plpgsql
            IMMUTABLE
                AS $function$ DECLARE         country_vector tsvector;         locality_vektor tsvector;         name_vektor tsvector; BEGIN     country_vector = to_tsvector('simple', country);     locality_vektor = to_tsvector('simple', locality);      IF locality_vektor = '' THEN         locality_vektor = country_vector;     END IF;      name_vektor = to_tsvector('simple', name);     IF name_vektor = '' THEN         name_vektor = locality_vektor;     END IF;     RETURN setweight(name_vektor, 'A') || setweight(locality_vektor, 'B')  || setweight(country_vector, 'C'); END $function$;";
            migrationBuilder.Sql(alterGetLocationsTsVector);
            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }
    }
}