using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillEnabledSupplierSettingsFromAgencySystemSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var enabledSuppliers = new List<string>
            {
                "netstorming",
                "illusions",
                "etg",
                "rakuten",
                "columbus",
                "jumeirah",
                "darina",
                "bronevik",
                "hotelbookPro",
                "bookMe"
            };

            // is required for insert to work
            migrationBuilder.Sql("update \"AgencySystemSettings\" set \"EnabledSuppliers\" = '{}'");
            
            // set true for every enabled supplier
            foreach (var supplier in enabledSuppliers)
            {
                migrationBuilder.Sql("update \"AgencySystemSettings\" set \"EnabledSuppliers\" = " +
                    " jsonb_set(\"EnabledSuppliers\"::jsonb, '{\"" + supplier + "\"}', to_jsonb(true))::jsonb" +
                    " where \"AccommodationBookingSettings\"->'EnabledSuppliers' @> '[\"" + supplier + "\"]'::jsonb");
            }
            
            // set false for every supplier not in the agency list
            foreach (var supplier in enabledSuppliers)
            {
                migrationBuilder.Sql("update \"AgencySystemSettings\" set \"EnabledSuppliers\" = " +
                    " jsonb_set(\"EnabledSuppliers\"::jsonb, '{\"" + supplier + "\"}', to_jsonb(false))::jsonb" +
                    " where not (\"AccommodationBookingSettings\"->'EnabledSuppliers' @> '[\"" + supplier + "\"]'::jsonb)");
            }
            
            // clean up
            migrationBuilder.Sql("update \"AgencySystemSettings\" set \"EnabledSuppliers\" = null" +
                " where \"AccommodationBookingSettings\" -> 'EnabledSuppliers' @> 'null'::jsonb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
