using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RefactorUserInvoicesDataToStrongTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<AdminInvitation.AdminInvitationData>(
                name: "Data",
                table: "UserInvitations",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "UserInvitations",
                type: "text",
                nullable: false,
                oldClrType: typeof(AdminInvitation.AdminInvitationData),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
