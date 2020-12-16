using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddIdToAppliedMarkups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppliedBookingMarkups",
                table: "AppliedBookingMarkups");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "AppliedBookingMarkups",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AppliedBookingMarkups",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppliedBookingMarkups",
                table: "AppliedBookingMarkups",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AppliedBookingMarkups",
                table: "AppliedBookingMarkups");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AppliedBookingMarkups");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "AppliedBookingMarkups",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppliedBookingMarkups",
                table: "AppliedBookingMarkups",
                columns: new[] { "ReferenceCode", "PolicyId" });
        }
    }
}
