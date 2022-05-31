﻿using System;
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
            var suppliers = new Dictionary<string, bool>()
            {
                {"rakuten", false},
                {"etg", false},
                {"travelgateXTest", false},
                {"netstorming", false},
                {"illusions", false},
                {"jumeirah", false},
                {"bronevik", false},
                {"avraTours", false},
                {"illusionsDirect", false},
                {"htTest", false},
                {"columbus", false},
                {"hotelBeds", false}
            };

            var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());
            var agentSettings = context.AgentSystemSettings.ToList();
            agentSettings.ForEach(a =>
                {
                    if (!Equals(a.AccommodationBookingSettings.EnabledSuppliers, null))
                        if (a.AccommodationBookingSettings.EnabledSuppliers.Count > 0)
                        {
                            a.EnabledSuppliers = suppliers;
                            a.AccommodationBookingSettings.EnabledSuppliers.ForEach(s => a.EnabledSuppliers[s] = true);
                        }
                }
            );

            context.UpdateRange(agentSettings);
            context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
