using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenamedServiceWorker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE public.""ServiceAccounts"" WHERE ""Id"" = 1
                SET ""ClientId"" ='service_worker'";

            migrationBuilder.Sql(sql);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"UPDATE public.""ServiceAccounts"" WHERE ""Id"" = 1
                SET ""ClientId"" ='serviceWorker'";

            migrationBuilder.Sql(sql);
        }
    }
}