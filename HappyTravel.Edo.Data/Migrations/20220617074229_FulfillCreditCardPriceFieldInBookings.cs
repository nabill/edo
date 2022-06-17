using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class FulfillCreditCardPriceFieldInBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var context = new EdoContextFactory().CreateDbContext(Array.Empty<string>());
            var bookings = context.Bookings.ToList();
            bookings.ForEach(b =>
                {
                    b.Rooms.ForEach(r =>
                    {
                        r.CreditCardPrice = r.Price;
                    });
                }
            );

            context.UpdateRange(bookings);
            context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
