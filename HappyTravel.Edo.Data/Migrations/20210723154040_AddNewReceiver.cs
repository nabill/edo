using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddNewReceiver : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"DefaultNotificationOptions\" " +
                "SET \"EnabledReceivers\" = 4 " +
                "WHERE \"Type\" = 32");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
