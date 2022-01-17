using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class ConfirmedBookingsRecordManager : IRecordManager<ConfirmedBookingsData>
    {
        public ConfirmedBookingsRecordManager(EdoContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<ConfirmedBookingsData>> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                orderby booking.CheckInDate
                where
                    booking.Status == BookingStatuses.Confirmed &&
                    booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                select new ConfirmedBookingsData
                {
                    Created = booking.Created.DateTime,
                    AccommodationName = booking.AccommodationName,
                    ReferenceCode = booking.ReferenceCode,
                    CheckInDate = booking.CheckInDate.DateTime,
                    CheckOutDate = booking.CheckOutDate.DateTime,
                    Rooms = booking.Rooms
                };
        }
        
        
        private readonly EdoContext _context;
    }
}