using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddTableAdministratorRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "AdministratorRoleIds",
                table: "Administrators",
                type: "integer[]",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdministratorRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministratorRoles", x => x.Id);
                });
            
            migrationBuilder.InsertData("AdministratorRoles", new string[] { "Name", "Permissions" }, new object[,]
            {
                {"Company administrator", 1 | 2 | 256 | 4096 | 65536},
                {"Accounts manager", 4 | 8 | 16 | 32 | 64 | 128 | 512 | 8192 | 32768},
                {"Booking manager", 1024 | 2048 | 16384},
                {"Auditor", 16384 | 32768 | 65536},
                {"Accommodation duplicates corrector", 1024 }
            });

            migrationBuilder.Sql("UPDATE \"Administrators\" " +
                "SET \"AdministratorRoleIds\" = '{1,2,3}'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministratorRoles");

            migrationBuilder.DropColumn(
                name: "AdministratorRoleIds",
                table: "Administrators");
        }
    }
}
