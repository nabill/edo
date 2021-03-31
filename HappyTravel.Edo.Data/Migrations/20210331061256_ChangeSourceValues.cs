using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSourceValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "UPDATE \"public\".\"BookingStatusHistory\" SET \"ChangeSource\" = '4' WHERE  \"ChangeSource\" = 2;" +
                "UPDATE \"public\".\"BookingStatusHistory\" SET \"ChangeSource\" = '1' WHERE  \"ChangeSource\" = 3";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
