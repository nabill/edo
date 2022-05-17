using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HappyTravel.Edo.Data.Migrations
{
    public partial class SetNetPriceForBookedRooms : Migration
    {
        public SetNetPriceForBookedRooms(EdoContext context)
        {
            _context = context;
        }


        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var bookings = _context.Bookings.ToList();
            bookings.ForEach(b =>
            {
                b.Rooms.ForEach(r =>
                {
                    r.NetPrice = r.Price;
                });
            });

            _context.UpdateRange(bookings);
            _context.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }


        private readonly EdoContext _context;
    }
}
