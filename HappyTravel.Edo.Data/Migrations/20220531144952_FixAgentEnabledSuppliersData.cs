using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixAgentEnabledSuppliersData : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE! : Commented because EnabledSuppliers field was already deleted! 
            // PR: https://github.com/happy-travel/edo/pull/1815
            // var suppliers = new Dictionary<string, bool>()
            // {
            //     {"rakuten", false},
            //     {"etg", false},
            //     {"netstorming", false},
            //     {"illusions", false},
            //     {"jumeirah", false},
            //     {"bronevik", false},
            //     {"illusionsDirect", false},
            //     {"darina", false},
            //     {"columbus", false},
            //     {"hotelbookPro", false},
            //     {"bookMe", false}
            // };

            // var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());
            // var agentSettings = context.AgentSystemSettings.Where(a => a.AccommodationBookingSettings != null).ToList();
            // agentSettings.ForEach(a =>
            //     {
            //         if (!Equals(a.AccommodationBookingSettings.EnabledSuppliers, null))
            //             if (a.AccommodationBookingSettings.EnabledSuppliers.Count > 0)
            //             {
            //                 a.EnabledSuppliers = new Dictionary<string, bool>(suppliers);
            //                 a.AccommodationBookingSettings.EnabledSuppliers.ForEach(s =>
            //                 {
            //                     if (a.EnabledSuppliers.ContainsKey(s))
            //                     {
            //                         a.EnabledSuppliers[s] = true;
            //                     }
            //                 });
            //             }
            //     }
            // );

            // context.UpdateRange(agentSettings);
            // context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
