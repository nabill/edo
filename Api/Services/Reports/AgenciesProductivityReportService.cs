using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public class AgenciesProductivityReportService : IAgenciesProductivityReportService
    {
        public AgenciesProductivityReportService(EdoContext context)
        {
            _context = context;
        }
        
        public async Task<Result<Stream>> GetReport(DateTime fromDate, DateTime endDate)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate);


            Result Validate()
            {
                if (fromDate == default || endDate == default)
                    return Result.Failure<Stream>("Range dates required");
            
                if ((endDate - fromDate).TotalDays > MaxDaysInReport)
                    return Result.Failure<Stream>($"The interval for generating a report should not exceed {MaxDaysInReport} days");

                return Result.Success();
            }
            
            
            IQueryable<AgencyProductivity> GetRecords()
            {
                var bookingsQuery = _context.Bookings
                    .Where(b => b.Created >= fromDate && b.Created < endDate);
                
                return from agency in _context.Agencies
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
                    };
            }
        }
        
        
        private async Task<Result<Stream>> Generate<TRow>(IEnumerable<TRow> records)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);
            
            _csvWriter.WriteHeader<TRow>();
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();
            
            foreach (var record in records)
            {
                _csvWriter.WriteRecord(record);
                await _csvWriter.NextRecordAsync();
                await _streamWriter.FlushAsync();
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        
        
        private const int MaxDaysInReport = 31;
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;
        
        private readonly EdoContext _context;


        public void Dispose()
        {
            _csvWriter?.Dispose();
            _streamWriter?.Dispose();
            _context?.Dispose();
        }
    }
}