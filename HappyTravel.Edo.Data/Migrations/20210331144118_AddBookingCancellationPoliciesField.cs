using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class AddBookingCancellationPoliciesField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<CancellationPolicy>>(
                name: "CancellationPolicies",
                table: "Bookings",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'[]'::jsonb");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationPolicies",
                table: "Bookings");
        }
    }
}
