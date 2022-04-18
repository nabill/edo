using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillEnabledSupplierSettingsFromAgencySystemSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var suppliers = new List<string>
            {
               "accor",
               "travelClick",
               "avraTours",
               "bakuun",
               "bookMe",
               "bronevik",
               "columbus",
               "ddHolidays",
               "darina",
               "directContracts",
               "grnConnect",
               "htTest",
               "hotelBeds",
               "hotelbookPro",
               "hyperGuest",
               "jumeirah",
               "mtsCityBreaks",
               "methabook",
               "netstorming",
               "nirvana",
               "nuitee",
               "paximum",
               "rakuten",
               "etg",
               "roibos",
               "sabre",
               "sodis",
               "solole",
               "travelLine",
               "travelgateXTest",
               "withinearth",
               "yalago",
               "illusions",
               "illusionsDirect"
            };

            foreach (var supplier in suppliers)
            {
                migrationBuilder.Sql("update \"AgencySystemSettings\" set \"EnabledSuppliers\" = " +
                    " jsonb_set(\"EnabledSuppliers\"::jsonb, '{\"" + supplier + "\"}', to_jsonb(true))::jsonb" +
                    " where \"AccommodationBookingSettings\"->'EnabledSuppliers' @> '[\"" + supplier + "\"]'::jsonb");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
