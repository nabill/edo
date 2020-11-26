using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveDefaultValueFromInAgencyPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 1 WHERE \"InAgencyPermissions\" = 2;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 2 WHERE \"InAgencyPermissions\" = 4;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4 WHERE \"InAgencyPermissions\" = 8;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 8 WHERE \"InAgencyPermissions\" = 16;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 16 WHERE \"InAgencyPermissions\" = 32;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 32 WHERE \"InAgencyPermissions\" = 64;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 64 WHERE \"InAgencyPermissions\" = 128;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 128 WHERE \"InAgencyPermissions\" = 256;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 256 WHERE \"InAgencyPermissions\" = 512;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 512 WHERE \"InAgencyPermissions\" = 1024;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 1024 WHERE \"InAgencyPermissions\" = 2048;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 2048 WHERE \"InAgencyPermissions\" = 4096;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4096 WHERE \"InAgencyPermissions\" = 8192;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 8192 WHERE \"InAgencyPermissions\" = 16384;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 16384 WHERE \"InAgencyPermissions\" = 32768;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 2 WHERE \"InAgencyPermissions\" = 1;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4 WHERE \"InAgencyPermissions\" = 2;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 8 WHERE \"InAgencyPermissions\" = 4;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 16 WHERE \"InAgencyPermissions\" = 8;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 32 WHERE \"InAgencyPermissions\" = 16;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 64 WHERE \"InAgencyPermissions\" = 32;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 128 WHERE \"InAgencyPermissions\" = 64;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 256 WHERE \"InAgencyPermissions\" = 128;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 512 WHERE \"InAgencyPermissions\" = 256;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 1024 WHERE \"InAgencyPermissions\" = 512;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 2048 WHERE \"InAgencyPermissions\" = 1024;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 4096 WHERE \"InAgencyPermissions\" = 2048;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 8192 WHERE \"InAgencyPermissions\" = 4096;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 16384 WHERE \"InAgencyPermissions\" = 8192;");
            migrationBuilder.Sql("UPDATE \"AgentAgencyRelations\" SET \"InAgencyPermissions\" = 32768 WHERE \"InAgencyPermissions\" = 16384;");
        }
    }
}
