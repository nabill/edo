using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class ThirdPartySuppliersReportManager : IRecordManager<ThirdPartySupplierData>
    {
        public ThirdPartySuppliersReportManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<ThirdPartySupplierData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                    orderby booking.Supplier, booking.Created
                    where 
                        booking.Created >= fromDate && booking.Created < endDate
                    select new ThirdPartySupplierData
                    {
                        Created = booking.Created.DateTime,
                        AccommodationName = booking.AccommodationName,
                        CheckInDate = booking.CheckInDate.DateTime,
                        CheckOutDate = booking.CheckOutDate.DateTime,
                        Supplier = booking.Supplier.ToString(),
                        DeadlineDate = booking.DeadlineDate.GetValueOrDefault().DateTime,
                        BookingStatus = booking.Status.ToString(),
                        ReferenceCode = booking.ReferenceCode,
                    })
                .ToListAsync();
        }
        
        private readonly EdoContext _context;
    }
}