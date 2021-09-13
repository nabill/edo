using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class HotelProductivityRecordManager : IRecordManager<HotelProductivityData>
    {
        public HotelProductivityRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<HotelProductivityData>> Get(DateTime fromDate, DateTime endDate)
        {
            var bookings = from booking in _context.Bookings
                where fromDate <= booking.CheckOutDate && booking.CheckOutDate < endDate
                    && booking.Status == BookingStatuses.Confirmed
                select new 
                {
                    booking.AccommodationName,
                    booking.HtId,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.Rooms,
                    booking.TotalPrice
                };

            var productivityData = from booking in await bookings.ToListAsync()
                group booking by booking.HtId
                into groupData
                select new HotelProductivityData
                {
                    AccommodationName = groupData.First().AccommodationName,
                    BookedNights = groupData.Sum(x => (int)(x.CheckOutDate - x.CheckInDate).TotalDays),
                    BookedRooms = groupData.Sum(x => x.Rooms.Count),
                    TotalRevenue = groupData.Sum(x => x.TotalPrice)
                };

            return productivityData.OrderByDescending(x => x.TotalRevenue);
        }


        private readonly EdoContext _context;
    }
}