using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FillBookingAvailabilityInfoFromAvailabilityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "update \"BookingRequests\" set \"AvailabilityData\" = '{\"availabilityId\":\"'||\"AvailabilityId\"||'\"}'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
