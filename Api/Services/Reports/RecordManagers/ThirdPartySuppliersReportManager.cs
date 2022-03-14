using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class ThirdPartySuppliersReportManager : IRecordManager<ThirdPartySupplierData>
    {
        public ThirdPartySuppliersReportManager(EdoContext context, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _context = context;
            _supplierOptionsStorage = supplierOptionsStorage;
        }
        
        
        public async Task<IEnumerable<ThirdPartySupplierData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                    orderby booking.SupplierCode, booking.Created
                    where 
                        booking.Created >= fromDate && booking.Created < endDate
                    select new ThirdPartySupplierData
                    {
                        Created = booking.Created.DateTime,
                        AccommodationName = booking.AccommodationName,
                        CheckInDate = booking.CheckInDate.DateTime,
                        CheckOutDate = booking.CheckOutDate.DateTime,
                        Supplier = GetSupplierName(_supplierOptionsStorage, booking.SupplierCode),
                        DeadlineDate = booking.DeadlineDate.GetValueOrDefault().DateTime,
                        BookingStatus = booking.Status.ToString(),
                        ReferenceCode = booking.ReferenceCode,
                    })
                .ToListAsync();
        }


        private static string GetSupplierName(ISupplierOptionsStorage storage, string code)
        {
            var (_, isFailure, supplier, _) = storage.Get(code);
            
            return isFailure
                ? string.Empty
                : supplier.Name;
        }
        
        private readonly EdoContext _context;
        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}