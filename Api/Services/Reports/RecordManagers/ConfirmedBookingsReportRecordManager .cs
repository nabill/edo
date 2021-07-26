using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class ConfirmedBookingsRecordManager : IRecordManager<ConfirmedBookingsProjection>
    {
        public ConfirmedBookingsRecordManager(EdoContext context)
        {
            _context = context;
        }


        public IQueryable<ConfirmedBookingsProjection> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                orderby booking.CheckInDate
                where
                    booking.Status == BookingStatuses.Confirmed &&
                    booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                select new ConfirmedBookingsProjection
                {
                    Created = booking.Created,
                    AccommodationName = booking.AccommodationName,
                    ReferenceCode = booking.ReferenceCode,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    Rooms = booking.Rooms
                };
        }
        
        
        private readonly EdoContext _context;
    }
}