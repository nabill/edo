using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemoveDataProviderFromSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var changeEnabledProvidersSqlAgents =
                "update \"AgentSystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"EnabledProviders\"', '\"EnabledSuppliers\"')::jsonb;\n";
            migrationBuilder.Sql(changeEnabledProvidersSqlAgents);

            var changeIsProviderVisibleSqlAgents = "update \"AgentSystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsDataProviderVisible\"', '\"IsSupplierVisible\"')::jsonb;\n";
            migrationBuilder.Sql(changeIsProviderVisibleSqlAgents);

            var changeEnabledProvidersSqlAgencies =
                "update \"AgencySystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"EnabledProviders\"', '\"EnabledSuppliers\"')::jsonb;\n";
            migrationBuilder.Sql(changeEnabledProvidersSqlAgencies);

            var changeIsProviderVisibleSqlAgencies =
                "update \"AgencySystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsDataProviderVisible\"', '\"IsSupplierVisible\"')::jsonb;";
            migrationBuilder.Sql(changeIsProviderVisibleSqlAgencies);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var changeEnabledProvidersSqlAgents =
                "update \"AgentSystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"EnabledSuppliers\"', '\"EnabledProviders\"')::jsonb;\n";
            migrationBuilder.Sql(changeEnabledProvidersSqlAgents);

            var changeIsProviderVisibleSqlAgents = "update \"AgentSystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsSupplierVisible\"', '\"IsDataProviderVisible\"')::jsonb;\n";
            migrationBuilder.Sql(changeIsProviderVisibleSqlAgents);

            var changeEnabledProvidersSqlAgencies =
                "update \"AgencySystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"EnabledSuppliers\"', '\"EnabledProviders\"')::jsonb;\n";
            migrationBuilder.Sql(changeEnabledProvidersSqlAgencies);

            var changeIsProviderVisibleSqlAgencies =
                "update \"AgencySystemSettings\"\nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsSupplierVisible\"', '\"IsDataProviderVisible\"')::jsonb;";
            migrationBuilder.Sql(changeIsProviderVisibleSqlAgencies);
        }
    }
}
