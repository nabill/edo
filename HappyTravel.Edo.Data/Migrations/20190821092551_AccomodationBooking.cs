using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AccomodationBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationBookingPassengers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    BookingRoomDetailsId = table.Column<int>(nullable: false),
                    Title = table.Column<int>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    IsLeader = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    Initials = table.Column<string>(nullable: true),
                    Age = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBookingPassengers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationBookingRoomDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AccommodationBookingId = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    IsExtraBedNeeded = table.Column<bool>(nullable: false),
                    IsCotNeededNeeded = table.Column<bool>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    ExtraBedPrice = table.Column<decimal>(nullable: false),
                    CotPrice = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBookingRoomDetails", x => x.Id);
                });

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
                    Features = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBookings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationBookingPassengers");

            migrationBuilder.DropTable(
                name: "AccommodationBookingRoomDetails");

            migrationBuilder.DropTable(
                name: "AccommodationBookings");
        }
    }
}
