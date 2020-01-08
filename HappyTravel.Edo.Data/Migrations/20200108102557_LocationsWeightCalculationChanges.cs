using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class LocationsWeightCalculationChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterGetLocationTsVector =
                "create or replace function get_location_tsvector(name jsonb, locality jsonb, country jsonb) returns tsvector     immutable     language plpgsql as $$ DECLARE         country_vector tsvector;         locality_vektor tsvector;         name_vektor tsvector; BEGIN     country_vector = to_tsvector('simple', country);     locality_vektor = to_tsvector('simple', locality);      IF locality_vektor = '' THEN         locality_vektor = country_vector;     END IF;      name_vektor = to_tsvector('simple', name);     IF name_vektor = '' THEN         name_vektor = locality_vektor;     END IF;     RETURN setweight(name_vektor, 'A') || setweight(locality_vektor, 'B')  || setweight(country_vector, 'C'); END $$;  alter function get_location_tsvector(jsonb, jsonb, jsonb) owner to postgres;  ";

            migrationBuilder.Sql(alterGetLocationTsVector);

            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var alterGetLocationTsVector =
                "create or replace function get_location_tsvector(name jsonb, locality jsonb, country jsonb) returns tsvector     immutable     language plpgsql as $$ BEGIN     RETURN setweight(to_tsvector('simple', name), 'A') || ' ' || setweight(to_tsvector('simple', locality), 'B')  || ' ' || setweight(to_tsvector('simple', country), 'C'); END $$;  alter function get_location_tsvector(jsonb, jsonb, jsonb) owner to postgres;  ";

            var reindexSql = "REINDEX INDEX \"IDX_GetLocationTsVector\"";
            migrationBuilder.Sql(reindexSql);
        }
    }
}
