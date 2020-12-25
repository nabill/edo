using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddReferenceCodeToPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var addReferenceCodeSql =
                "update \"Payments\" p\nset \"ReferenceCode\" = (select \"ReferenceCode\" from \"Bookings\" where \"Bookings\".\"Id\" = p.\"BookingId\") where p.\"ReferenceCode\" IS NULL;";
            migrationBuilder.Sql(addReferenceCodeSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
