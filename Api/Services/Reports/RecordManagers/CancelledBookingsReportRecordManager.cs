using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.SupplierOptionsProvider;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class CancelledBookingsReportRecordManager : IRecordManager<CancelledBookingsReportData>
    {
        public CancelledBookingsReportRecordManager(EdoContext context, ISupplierOptionsStorage supplierOptionsStorage)
        {
            _context = context;
            _supplierOptionsStorage = supplierOptionsStorage;
        }
        
        
        public async Task<IEnumerable<CancelledBookingsReportData>> Get(DateTime fromDate, DateTime endDate)
        {
            var cancelledBookings = from booking in _context.Bookings
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join agent in _context.Agents on booking.AgentId equals agent.Id
                where booking.Created >= fromDate && booking.Created < endDate && booking.Status == BookingStatuses.Cancelled
                select new CancelledBookingsReportData
                {
                    Created = booking.Created.DateTime,
                    AccommodationName = booking.AccommodationName,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    AgencyName = agency.Name,
                    CheckInDate = booking.CheckInDate.DateTime,
                    CheckOutDate = booking.CheckOutDate.DateTime,
                    Supplier = GetSupplierName(_supplierOptionsStorage, booking.SupplierCode),
                    DeadlineDate = booking.DeadlineDate.GetValueOrDefault().DateTime,
                    ReferenceCode = booking.ReferenceCode,
                };

            return await cancelledBookings.ToListAsync();
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