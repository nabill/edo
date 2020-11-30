using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddCreditCardPaymentConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreditCardPaymentConfirmations",
                columns: table => new
                {
                    BookingId = table.Column<int>(nullable: false),
                    AgentId = table.Column<int>(nullable: false),
                    ConfirmedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardPaymentConfirmations", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_CreditCardPaymentConfirmations_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CreditCardPaymentConfirmations");
        }
    }
}
