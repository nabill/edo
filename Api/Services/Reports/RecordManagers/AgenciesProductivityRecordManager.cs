using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class AgenciesProductivityRecordManager : IRecordManager<AgencyProductivity>
    {
        public AgenciesProductivityRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<AgencyProductivity>> Get(DateTime fromDate, DateTime endDate)
        {
            var bookingsQuery = _context.Bookings
                .Where(b => b.Created >= fromDate && b.Created < endDate);

            return await (from agency in _context.Agencies
                join bookings in bookingsQuery on agency.Id equals bookings.AgencyId into joined
                from booking in joined.DefaultIfEmpty()
                group booking by new
                {
                    agency.Name,
                    agency.IsActive,
                    booking.Currency,
                    booking.AccommodationName,
                    booking.Location.Locality,
                    booking.Location.Country
                } into grouped
                select new AgencyProductivity
                {
                    AgencyName = grouped.Key.Name,
                    BookingCount = grouped.Count(b => b != null),
                    Currency = grouped.Key.Currency.ToString(),
                    Revenue = grouped.Sum(b => b.TotalPrice),
                    NightCount = grouped.Sum(b => (b.CheckOutDate - b.CheckInDate).Days),
                    IsActive = grouped.Key.IsActive,
                    CountryName = grouped.Key.Country,
                    LocalityName = grouped.Key.Locality,
                    AccommodationName = grouped.Key.AccommodationName
                })
                .ToListAsync();
        }


        private readonly EdoContext _context;
    }
}