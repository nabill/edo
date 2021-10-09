using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddNotificationOption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO \"DefaultNotificationOptions\" VALUES (33, 2, TRUE, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
