using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdateAgentSupplierSettingsToCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var addProcedureSql = "CREATE OR REPLACE FUNCTION replace_supplier_ids_in_array(suppliersArray jsonb)\n    RETURNS jsonb AS $$\nDECLARE\n    id text;\n    code text;\n    resultArray jsonb;\n    mapping jsonb;\nBEGIN\n    mapping = '{\"1\":\"netstorming\",\"2\":\"illusions\",\"3\":\"directContracts\",\"4\":\"etg\",\"5\":\"rakuten\",\"6\":\"columbus\",\"7\":\"travelgateXTest\",\"8\":\"jumeirah\",\"9\":\"darina\",\"10\":\"yalago\",\"11\":\"paximum\",\"12\":\"travelLine\",\"13\":\"bronevik\",\"14\":\"travelClick\",\"15\":\"sodis\",\"16\":\"hotelBeds\",\"17\":\"hotelbookPro\",\"18\":\"bookMe\",\"19\":\"roibos\",\"20\":\"sabre\",\"21\":\"bakuun\",\"22\":\"accor\",\"23\":\"nirvana\",\"24\":\"nuitee\",\"25\":\"hyperGuest\",\"26\":\"solole\",\"27\":\"grnConnect\",\"28\":\"methabook\",\"29\":\"withinearth\",\"30\":\"mtsCityBreaks\",\"31\":\"avraTours\",\"32\":\"ddHolidays\",\"33\":\"htTest\",\"34\":\"illusionsDirect\"}'::jsonb;\n    resultArray = '[]'::jsonb;\n    for id in (select jsonb_array_elements(suppliersArray) element)\n        loop\n            code = jsonb_extract_path(mapping::jsonb, id);\n            if (id = '0') then\n              continue;\n            end if;\n            \n            resultArray = jsonb_concat(resultArray, code::jsonb);\n        end loop;\n\n    return resultArray;\nEND\n$$ LANGUAGE 'plpgsql' IMMUTABLE;";
            migrationBuilder.Sql(addProcedureSql);

            var updateAgentSettingsSql =
                "update \"AgentSystemSettings\"\nset \"AccommodationBookingSettings\" = jsonb_set(\"AccommodationBookingSettings\", '{EnabledSuppliers}', replace_supplier_ids_in_array(\"AccommodationBookingSettings\"->'EnabledSuppliers')) "+
                "where \"AccommodationBookingSettings\"->'EnabledSuppliers'::text <> 'null';";

            migrationBuilder.Sql(updateAgentSettingsSql);
            var updateAgencySettingsSql =
                "update \"AgencySystemSettings\"\nset \"AccommodationBookingSettings\" = jsonb_set(\"AccommodationBookingSettings\", '{EnabledSuppliers}', replace_supplier_ids_in_array(\"AccommodationBookingSettings\"->'EnabledSuppliers'))" +
                "where \"AccommodationBookingSettings\"->'EnabledSuppliers'::text <> 'null';";

            migrationBuilder.Sql(updateAgencySettingsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
