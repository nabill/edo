using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenamedServiceWorker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE public.""ServiceAccounts"" 
                SET ""ClientId"" ='service_worker'
                WHERE ""Id"" = 1";

            migrationBuilder.Sql(sql);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE public.""ServiceAccounts""
                SET ""ClientId"" ='serviceWorker'
                 WHERE ""Id"" = 1";

            migrationBuilder.Sql(sql);
        }
    }
}