using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ConvertDataToPascalCaseInUserInvitations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string convertToPascalCaseFunction = @"
                create or replace function json_keys_to_pascal_case(
                    p_json  jsonb
                ) returns jsonb
                as $$
                declare
                    t_json   text;
                    t_match  text[];
                    t_pascal  text;
                begin
                    t_json := p_json::text;
                    for t_match in (select regexp_matches(t_json, '(""([a-zA-Z]+)"":)', 'g')) loop
                            t_pascal := upper(substring(t_match[2] from 1 for 1)) || substring(t_match[2] from 2 for length(t_match[2]));
                            t_json := replace(t_json, t_match[1], '""' || t_pascal || '"":');
                            end loop;
                            return t_json::jsonb;
                            end;
                                $$ language plpgsql stable;
            ";

            const string convertToCamelCaseFunction = @"
                create or replace function json_keys_to_camel_case(
                    p_json  jsonb
                ) returns jsonb
                as $$
                declare
                    t_json   text;
                    t_match  text[];
                    t_camel  text;
                begin
                    t_json := p_json::text;
                    for t_match in (select regexp_matches(t_json, '(""([a-zA-Z]+)"":)', 'g')) loop
                            t_camel := lower(substring(t_match[2] from 1 for 1)) || substring(t_match[2] from 2 for length(t_match[2]));
                            t_json := replace(t_json, t_match[1], '""' || t_camel || '"":');
                            end loop;
                            return t_json::jsonb;
                            end;
                                $$ language plpgsql stable;
            ";

            migrationBuilder.Sql(convertToPascalCaseFunction);
            migrationBuilder.Sql(convertToCamelCaseFunction);
            migrationBuilder.Sql(@"UPDATE public.""UserInvitations"" SET ""Data"" = json_keys_to_pascal_case(""Data"")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE public.""UserInvitations"" SET ""Data"" = json_keys_to_camel_case(""Data"")");
        }
    }
}
