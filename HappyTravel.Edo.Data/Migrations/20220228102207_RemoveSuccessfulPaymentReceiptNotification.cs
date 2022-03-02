using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveSuccessfulPaymentReceiptNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{19, 22, 33}' where \"Id\" = 2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
