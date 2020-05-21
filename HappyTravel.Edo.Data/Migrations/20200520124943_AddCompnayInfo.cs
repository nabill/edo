using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCompnayInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql =
                "INSERT INTO public.\"StaticData\" (\"Type\", \"Data\") VALUES (1, '{\"Name\":\"HappyTravelDotCom Travel and Tourism LLC\",\"Address\":\"B105, Saraya Avenue building,Garhoud, Deira\",\"Country\":\"United Arab Emirates\",\"City\":\"Dubai\",\"Phone\":\"+971-4-2940007\",\"Email\":\"info@happytravel.com\",\"PostalCode\":\"Box 36690\",\"TRN\":\"100497287100003\",\"IATA\":\"96-0 4653\",\"TradeLicense\":\"828719\"}')";
            migrationBuilder.Sql(sql);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = "DELETE FROM public.\"StaticData\" WHERE TYPE =1";
            migrationBuilder.Sql(sql);
        }
    }
}