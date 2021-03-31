using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ChangeSourceValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "UPDATE \"public\".\"BookingStatusHistory\" SET \"ChangeSource\" = '0' WHERE  \"ChangeSource\" = 2";

            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "UPDATE \"public\".\"BookingStatusHistory\" SET \"ChangeSource\" = '0' WHERE  \"ChangeSource\" = 2";

            migrationBuilder.Sql(sql);
        }
    }
}
