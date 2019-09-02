using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class CustomerInvitationsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerInvitations",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    IsAccepted = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInvitations", x => x.Code);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerInvitations");
        }
    }
}
