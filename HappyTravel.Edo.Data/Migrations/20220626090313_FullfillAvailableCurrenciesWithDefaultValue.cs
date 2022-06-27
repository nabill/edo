using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HappyTravel.Edo.Data.StaticData;
using HappyTravel.Money.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FullfillAvailableCurrenciesWithDefaultValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());

            var defaultCurrency = Currencies.USD;
            var jsonDocument = context.StaticData
                .Where(s => s.Type == StaticDataTypes.CompanyInfo)
                .Select(s => s.Data)
                .FirstOrDefault();
            if (jsonDocument != default)
            {
                JsonElement jsonElement = default;
                var isExist = jsonDocument.RootElement.TryGetProperty("DefaultCurrency", out jsonElement);
                if (isExist)
                    defaultCurrency = jsonElement.Deserialize<Currencies>();
            }

            var systemSettings = context.AgencySystemSettings
                .Where(s => s.AccommodationBookingSettings != null)
                .ToList();

            systemSettings.ForEach(s =>
            {
                s.AccommodationBookingSettings.AvailableCurrencies = new List<Currencies>() { defaultCurrency };
            });

            context.UpdateRange(systemSettings);
            context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
