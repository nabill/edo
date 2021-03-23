using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeInternalProcessingEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Setting manual correction needed status for these bookings to check what to do for each
            // Sometimes these bookings are just created and sometimes they really got this status from the supplier
            var sql = "UPDATE \"Bookings\" SET \"Status\" = 8 WHERE \"Status\" = 0";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
