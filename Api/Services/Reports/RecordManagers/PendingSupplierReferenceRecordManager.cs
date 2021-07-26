using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class PendingSupplierReferenceRecordManager : IRecordManager<PendingSupplierReferenceProjection>
    {
        public PendingSupplierReferenceRecordManager(EdoContext context)
        {
            _context = context;
        }


        public IQueryable<PendingSupplierReferenceProjection> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                orderby booking.CheckInDate
                where
                    (booking.Status == BookingStatuses.Pending || booking.Status == BookingStatuses.ManualCorrectionNeeded) &&
                    booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                select new PendingSupplierReferenceProjection
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