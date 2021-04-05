using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class ReplaceTagsVisibilityFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var changeAgentSystemSettingsSql =
                "update \"AgentSystemSettings\" \nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"AreTagsVisible\"', '\"IsDirectContractFlagVisible\"')::jsonb";
            
            var changeAgencySystemSettingsSql =
                "update \"AgencySystemSettings\" \nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"AreTagsVisible\"', '\"IsDirectContractFlagVisible\"')::jsonb";

            migrationBuilder.Sql(changeAgentSystemSettingsSql);
            migrationBuilder.Sql(changeAgencySystemSettingsSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var changeAgentSystemSettingsSql =
                "update \"AgentSystemSettings\" \nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsDirectContractFlagVisible\"', '\"AreTagsVisible\"')::jsonb";
            
            var changeAgencySystemSettingsSql =
                "update \"AgencySystemSettings\" \nset \"AccommodationBookingSettings\" = replace(\"AccommodationBookingSettings\"::text, '\"IsDirectContractFlagVisible\"', '\"AreTagsVisible\"')::jsonb";

            migrationBuilder.Sql(changeAgentSystemSettingsSql);
            migrationBuilder.Sql(changeAgencySystemSettingsSql);
        }
    }
}
