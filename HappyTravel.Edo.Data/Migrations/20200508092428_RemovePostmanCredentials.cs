using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RemovePostmanCredentials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"Agents\" WHERE \"Id\" = -1");
            migrationBuilder.Sql("DELETE FROM public.\"Agencies\" WHERE \"Id\" = -1");
            migrationBuilder.Sql("DELETE FROM public.\"Counterparties\" WHERE \"Id\" = -1");
            migrationBuilder.Sql("DELETE FROM public.\"AgentCounterpartyRelations\" WHERE \"CounterpartyId\" = -1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
