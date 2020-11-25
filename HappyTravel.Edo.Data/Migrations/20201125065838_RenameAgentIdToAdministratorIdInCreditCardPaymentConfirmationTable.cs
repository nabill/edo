using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class RenameAgentIdToAdministratorIdInCreditCardPaymentConfirmationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "AgentId",
                "CreditCardPaymentConfirmations",
                "AdministratorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "AdministratorId",
                "CreditCardPaymentConfirmations",
                "AgentId");
        }
    }
}
