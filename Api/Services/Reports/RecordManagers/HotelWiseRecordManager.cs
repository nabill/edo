using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class HotelWiseRecordManager : IRecordManager<HotelWiseData>
    {
        public HotelWiseRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<HotelWiseData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                    orderby booking.AccommodationName, booking.CheckInDate
                    where 
                        booking.Created >= fromDate && booking.Created < endDate
                    select new HotelWiseData
                    {
                        Created = booking.Created,
                        AccommodationName = booking.AccommodationName,
                        CheckInDate = booking.CheckInDate,
                        CheckOutDate = booking.CheckOutDate,
                        BookingStatus = booking.Status,
                        ReferenceCode = booking.ReferenceCode,
                        Rooms = booking.Rooms
                    })
                .ToListAsync();
        }
        
        private readonly EdoContext _context;
    }
}