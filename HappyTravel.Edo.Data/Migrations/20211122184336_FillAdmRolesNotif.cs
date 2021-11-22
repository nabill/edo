using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillAdmRolesNotif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{16, 17}' where \"Id\" = 1");
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{4, 19, 22}' where \"Id\" = 2");
            migrationBuilder.Sql("update \"AdministratorRoles\" set \"NotificationTypes\" = '{2, 6, 7, 23, 18}' where \"Id\" = 3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
