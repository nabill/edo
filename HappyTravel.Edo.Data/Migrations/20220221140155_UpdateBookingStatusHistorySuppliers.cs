using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class UpdateBookingStatusHistorySuppliers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (from, to) in FromToMapping)
            {
                migrationBuilder.Sql($@"
                UPDATE ""BookingStatusHistory"" 
                SET ""UserId"" = '{to}'
                WHERE ""UserId"" = '{from}' AND ""ApiCallerType"" = 5");
            }

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (to, from) in FromToMapping)
            {
                migrationBuilder.Sql($@"
                UPDATE ""BookingStatusHistory"" 
                SET ""UserId"" = '{to}'
                WHERE ""UserId"" = '{from}' AND ""ApiCallerType"" = 5");
            }
        }
        
        
        private static readonly (int, string)[] FromToMapping = new[]
        {
            (1, "netstorming"),
            (2, "illusions"),
            (3, "directContracts"),
            (4, "etg"),
            (5, "rakuten"),
            (6, "columbus"),
            (7, "travelgateXTest"),
            (8, "jumeirah"),
            (9, "darina"),
            (10, "yalago"),
            (11, "paximum"),
            (12, "travelLine"),
            (13, "bronevik"),
            (14, "travelClick"),
            (15, "sodis"),
            (16, "hotelBeds"),
            (17, "hotelbookPro"),
            (18, "bookMe"),
            (19, "roibos"),
            (20, "sabre"),
            (21, "bakuun"),
            (22, "accor"),
            (23, "nirvana"),
            (24, "nuitee"),
            (25, "hyperGuest"),
            (26, "solole"),
            (27, "grnConnect"),
            (28, "methabook"),
            (29, "withinearth"),
            (30, "mtsCityBreaks"),
            (31, "avraTours"),
            (32, "ddHolidays"),
            (33, "htTest")
        };
    }
}
