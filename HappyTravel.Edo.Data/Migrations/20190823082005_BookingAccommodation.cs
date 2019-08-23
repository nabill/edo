using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class BookingAccommodation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationBookings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CustomerId = table.Column<int>(nullable: false),
                    AgentReference = table.Column<string>(nullable: true),
                    ReferenceCode = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    BookingDate = table.Column<DateTime>(nullable: false),
                    PriceCurrency = table.Column<int>(nullable: false),
                    CheckInDate = table.Column<DateTime>(nullable: false),
                    CheckOutDate = table.Column<DateTime>(nullable: false),
                    CityCode = table.Column<string>(nullable: false),
                    AccommodationId = table.Column<string>(nullable: true),
                    TariffCode = table.Column<string>(nullable: false),
                    ContractTypeId = table.Column<int>(nullable: false),
                    Deadline = table.Column<DateTime>(nullable: false),
                    Nationality = table.Column<string>(nullable: true),
                    Residency = table.Column<string>(nullable: true),
                    Service = table.Column<string>(nullable: true),
                    RateBasis = table.Column<string>(nullable: true),
                    CountryCode = table.Column<string>(nullable: true),
                    Features = table.Column<string>(type: "jsonb", nullable: true),
                    ItineraryNumber = table.Column<long>(nullable: false),
                    MainPassengerName = table.Column<string>(nullable: false),
                    CompanyId = table.Column<int>(nullable: false),
                    RoomDetails = table.Column<string>(type: "jsonb", nullable: true),
                    PaymentMethod = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBookings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_CheckInDate",
                table: "AccommodationBookings",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_CheckOutDate",
                table: "AccommodationBookings",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_CompanyId",
                table: "AccommodationBookings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_CustomerId",
                table: "AccommodationBookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_ItineraryNumber",
                table: "AccommodationBookings",
                column: "ItineraryNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_MainPassengerName",
                table: "AccommodationBookings",
                column: "MainPassengerName");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBookings_ReferenceCode",
                table: "AccommodationBookings",
                column: "ReferenceCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationBookings");
        }
    }
}
