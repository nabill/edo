using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class CancellationDeadlineReportManager : IRecordManager<CancellationDeadlineData>
    {
        public CancellationDeadlineReportManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<CancellationDeadlineData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                    orderby booking.DeadlineDate
                    where 
                        fromDate <= booking.CheckInDate && booking.CheckOutDate < endDate
                    select new CancellationDeadlineData
                    {
                        Created = booking.Created.DateTime,
                        AccommodationName = booking.AccommodationName,
                        CheckInDate = booking.CheckInDate.DateTime,
                        CheckOutDate = booking.CheckOutDate.DateTime,
                        DeadlineDate = booking.DeadlineDate.GetValueOrDefault().DateTime,
                        BookingStatus = booking.Status.ToString(),
                        ReferenceCode = booking.ReferenceCode,
                    })
                .ToListAsync();
        }
        
        private readonly EdoContext _context;
    }
}