using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FixNationalityCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Bookings"" 
                    SET ""Nationality"" = upper(""Nationality"")
                WHERE char_length(""Nationality"") = 2
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""Bookings"" 
                    SET ""Residency"" = upper(""Residency"")
                WHERE char_length(""Residency"") = 2
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
