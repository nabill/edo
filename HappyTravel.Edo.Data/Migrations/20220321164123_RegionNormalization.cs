using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RegionNormalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Generated automatically
            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "Agencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Delete all regions
            var deleteAllRegionsSql = "FROM \"Regions\" WHERE \"Id\" <> -1;";
            migrationBuilder.Sql(deleteAllRegionsSql);

            #region Set "Unknown" region to all countries and agencies
            var unknown = RegionCountriesMapper.First(rc => rc.Item1.Id.Equals(1));

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Names" },
                values: new object[,] { { unknown.Item1.Id, JsonDocument.Parse(unknown.Item1.Names) } }
            );

            migrationBuilder.Sql(@"UPDATE ""Countries""
            SET ""RegionId"" = 1");

            migrationBuilder.Sql(@"UPDATE ""Agencies""
            SET ""RegionId"" = 1");
            #endregion

            #region Add new regions and match countries and agencies with them
            foreach (var (region, countries) in RegionCountriesMapper.Where(rc => rc.Item1.Id.Equals(1)))
            {
                migrationBuilder.InsertData(
                    table: "Regions",
                    columns: new[] { "Id", "Names" },
                    values: new object[,] { { region.Id, JsonDocument.Parse(region.Names) } }
                );

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
            #endregion
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Agencies");
        }


        private static readonly List<(InternalRegion, string[])> RegionCountriesMapper =
            new List<(InternalRegion, string[])>
        {
            (new InternalRegion("Unknown"), null),
            (new InternalRegion("Africa"), new []
                {
                    "DZ", "AO", "BJ", "BW", "BF", "BI", "CM", "CV", "CF",
                    "TD", "KM", "CG", "CD", "CI", "DJ", "GQ", "ER", "ET",
                    "GA", "GM", "GH", "GN", "GW", "KE", "LS", "LR", "LY",
                    "MG", "MW", "ML", "MR", "MU", "YT", "MA", "MZ", "NA",
                    "NE", "NG", "RE", "RW", "SH", "ST", "SN", "SC", "SL",
                    "SO", "ZA", "SS", "SD", "SZ", "TZ", "TG", "TN", "TN",
                    "EH", "ZM", "ZW"
                }),
            (new InternalRegion("Far east"), new []
                {
                    "IO", "BN", "KH", "CN", "TF", "HK", "ID", "JP", "LA",
                    "MO", "MY", "MN", "MM", "KR", "PH", "PW", "SG", "KP",
                    "TW", "TH", "TL", "VN"
                }),
            (new InternalRegion("Gcc"), new []
                {
                    "BH", "KW", "OM", "QA", "SA", "AE"
                }),
            (new InternalRegion("Middle east"), new []
                {
                    "EG", "IR", "IQ", "IL", "JO", "LB", "PS", "SY", "YE"
                }),
            (new InternalRegion("Sub continent"), new []
                {
                    "AF", "BD", "BT", "IN", "MV", "NP", "PK", "LK"
                }),
            (new InternalRegion("Europe"), new []
                {
                    "AX", "AL", "AD", "AU", "BE", "BA", "BV", "BG", "HR",
                    "CY", "CZ", "DK", "FO", "FI", "FR", "DE", "GI", "GR",
                    "GG", "HU", "IS", "IM", "IT", "JE", "LI", "LU", "MK",
                    "MT", "MC", "ME", "NL", "NO", "PL", "PT", "IE", "RO",
                    "SM", "RS", "SK", "SI", "ES", "SJ", "SE", "CH", "TR",
                    "GB", "VA"
                }),
            (new InternalRegion("Cis"), new []
                {
                    "AM", "AZ", "BY", "EE", "GE", "KZ", "KG", "LV", "LT",
                    "MD", "RU", "TJ", "TM", "UA", "UZ"
                }),
            (new InternalRegion("Oceania"), new []
                {
                    "AS", "AU", "CX", "CC", "CK", "FJ", "PF", "GU", "HM",
                    "KI", "MH", "FM", "NR", "NC", "NZ", "NU", "NF", "MP",
                    "PG", "PN", "WS", "SB", "TK", "TO", "TV", "VU", "WF"
                }),
            (new InternalRegion("North america"), new []
                {
                    "AQ", "BM", "CA", "GL", "PM", "US", "UM"
                }),
            (new InternalRegion("Latin america"), new []
                {
                    "AI", "AG", "AR", "AW", "BS", "BB", "BZ", "BO", "BQ",
                    "BR", "KY", "CL", "CO", "CR", "CU", "CW", "DM", "DO",
                    "EC", "SV", "FK", "GF", "GD", "GP", "GT", "GY", "HT",
                    "HN", "JM", "MQ", "MX", "MS", "NI", "PA", "PE", "PR",
                    "BL", "KN", "MF", "VC", "SX", "GS", "LC", "SR", "TT",
                    "TC", "VI", "VG", "UY", "VE"
                }),
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