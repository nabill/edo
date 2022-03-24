using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixUnmatchedContries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            #region Match regions with missed countries and agencies
            foreach (var (region, countries) in RegionCountriesMapper.Where(rc => rc.Item2 is not null))
            {
                if (countries.Length > 0)
                {
                    migrationBuilder.UpdateData(
                        table: "Countries",
                        keyColumn: "Code",
                        keyValues: countries,
                        column: "RegionId",
                        values: Enumerable.Repeat<object>(region.Id, countries.Count()).ToArray()
                    );

                    // Set region to each agencies by country codes
                    migrationBuilder.UpdateData(
                        table: "Agencies",
                        keyColumn: "CountryCode",
                        keyValues: countries,
                        column: "RegionId",
                        values: Enumerable.Repeat<object>(region.Id, countries.Count()).ToArray()
                    );
                }
            }
            #endregion
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }


        private static readonly List<(InternalRegion, string[])> RegionCountriesMapper =
            new List<(InternalRegion, string[])>
        {
            (new InternalRegion("Unknown"), null),
            (new InternalRegion("Africa"), new []
                {
                    "UG"
                }),
            (new InternalRegion("Far East"), null),
            (new InternalRegion("GCC"), null),
            (new InternalRegion("Middle East"), null),
            (new InternalRegion("Subcontinent"), null),
            (new InternalRegion("Europe"), new []
                {
                    "AT"
                }),
            (new InternalRegion("CIS"), null),
            (new InternalRegion("Oceania"), null),
            (new InternalRegion("North America"), null),
            (new InternalRegion("Latin America"), new []
                {
                    "PY"
                })
        };


        private class InternalRegion
        {
            public InternalRegion(string names)
            {
                Id = ++counter;
                Names = names;
            }


            public int Id { get; init; }

            public string Names
            {
                get => _names;
                set => _names = "{ \"en\": \"" + value + "\"}";
            }

            private string _names;

            private static int counter = 0;
        }
    }
}
