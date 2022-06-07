using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveUnusedEnabledSuppliers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var command = "UPDATE \"AgentSystemSettings\"" +
                " SET \"AccommodationBookingSettings\" = \"AccommodationBookingSettings\"::jsonb - 'EnabledSuppliers'";
            migrationBuilder.Sql(command);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
