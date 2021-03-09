using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddInvitationStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "UserInvitations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvitationStatus",
                table: "UserInvitations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "InviterAgencyId",
                table: "UserInvitations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InviterUserId",
                table: "UserInvitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"UserInvitations\" " +
                "SET \"InvitationStatus\" = CASE " +
                "        WHEN NOT \"IsActive\" THEN 2" +
                "        WHEN COALESCE(\"IsResent\", FALSE) THEN 3" +
                "        WHEN \"IsAccepted\" THEN 4" +
                "        WHEN \"IsActive\" AND NOT COALESCE(\"IsResent\", FALSE) AND NOT \"IsAccepted\" THEN 1" +
                "    END");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "IsResent",
                table: "UserInvitations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "UserInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsResent",
                table: "UserInvitations",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"UserInvitations\" SET \"IsActive\" = TRUE, \"IsResent\" = FALSE, \"IsAccepted\" = FALSE WHERE \"InvitationStatus\" = 1");

            migrationBuilder.Sql(
                "UPDATE \"UserInvitations\" SET \"IsActive\" = FALSE, \"IsResent\" = FALSE, \"IsAccepted\" = FALSE WHERE \"InvitationStatus\" = 2");

            migrationBuilder.Sql(
                "UPDATE \"UserInvitations\" SET \"IsActive\" = TRUE, \"IsResent\" = TRUE, \"IsAccepted\" = FALSE WHERE \"InvitationStatus\" = 3");

            migrationBuilder.Sql(
                "UPDATE \"UserInvitations\" SET \"IsActive\" = TRUE, \"IsResent\" = FALSE, \"IsAccepted\" = TRUE WHERE \"InvitationStatus\" = 4");

            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "UserInvitations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "InvitationStatus",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "InviterAgencyId",
                table: "UserInvitations");

            migrationBuilder.DropColumn(
                name: "InviterUserId",
                table: "UserInvitations");
        }
    }
}
