using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class NormalizeAgentsEnabledSuppliers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE! : Commented because EnabledSuppliers field was already deleted!
            // PR: https://github.com/happy-travel/edo/pull/1815
            // var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());
            // var agentSettings = context.AgentSystemSettings.ToList();
            // agentSettings.ForEach(a => 
            //     {
            //         if(!Equals(a.AccommodationBookingSettings.EnabledSuppliers, null))
            //             if(a.AccommodationBookingSettings.EnabledSuppliers.Count > 0)
            //             {
            //                 a.EnabledSuppliers = new Dictionary<string, bool>();
            //                 a.AccommodationBookingSettings.EnabledSuppliers.ForEach(s => a.EnabledSuppliers.Add(s, true));
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
