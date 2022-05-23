using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class DeleteEnabledSuppliersFromNestedJsonbSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = "UPDATE \"AgencySystemSettings\" SET \"AccommodationBookingSettings\" = \"AccommodationBookingSettings\"::jsonb - 'EnabledSuppliers'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
