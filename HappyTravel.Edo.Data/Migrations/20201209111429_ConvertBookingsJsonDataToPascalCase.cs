using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ConvertBookingsJsonDataToPascalCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE public.""Bookings"" SET ""Location"" = json_keys_to_pascal_case(""Location"")");
            migrationBuilder.Sql(@"UPDATE public.""Bookings"" SET ""Rooms"" = json_keys_to_pascal_case(""Rooms"")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE public.""Bookings"" SET ""Location"" = json_keys_to_camel_case(""Location"")");
            migrationBuilder.Sql(@"UPDATE public.""Bookings"" SET ""Rooms"" = json_keys_to_camel_case(""Rooms"")");
        }
    }
}
