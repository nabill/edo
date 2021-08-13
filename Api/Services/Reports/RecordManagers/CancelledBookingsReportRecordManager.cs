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
    public class CancelledBookingsReportRecordManager : IRecordManager<CancelledBookingsData>
    {
        public CancelledBookingsReportRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<CancelledBookingsData>> Get(DateTime fromDate, DateTime endDate)
        {
            var cancelledBookings = from booking in _context.Bookings
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join agent in _context.Agents on booking.AgentId equals agent.Id
                where booking.Created >= fromDate && booking.Created < endDate && booking.Status == BookingStatuses.Cancelled
                select new CancelledBookingsData
                {
                    Created = booking.Created,
                    AccommodationName = booking.AccommodationName,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    AgencyName = agency.Name,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    Supplier = booking.Supplier.ToString(),
                    DeadlineDate = booking.DeadlineDate.GetValueOrDefault(),
                    ReferenceCode = booking.ReferenceCode,
                };

            return await cancelledBookings.ToListAsync();
        }


        private readonly EdoContext _context;
    }
}