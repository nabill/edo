using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSourceValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "UPDATE \"public\".\"BookingStatusHistory\" SET \"Source\" = '4' WHERE  \"Source\" = 2;" +
                "UPDATE \"public\".\"BookingStatusHistory\" SET \"Source\" = '1' WHERE  \"Source\" = 3";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
