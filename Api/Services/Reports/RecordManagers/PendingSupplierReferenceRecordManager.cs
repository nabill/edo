using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class PendingSupplierReferenceRecordManager : IRecordManager<PendingSupplierReferenceData>
    {
        public PendingSupplierReferenceRecordManager(EdoContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<PendingSupplierReferenceData>> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                orderby booking.CheckInDate
                where
                    (booking.Status == BookingStatuses.Pending || 
                        booking.Status == BookingStatuses.ManualCorrectionNeeded ||
                        booking.Status == BookingStatuses.WaitingForResponse ||
                        booking.Status == BookingStatuses.PendingCancellation
                        ) &&
                    booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                select new PendingSupplierReferenceData
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