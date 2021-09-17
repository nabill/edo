using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Api.Services.Reports.Converters;
using HappyTravel.Edo.Api.Services.Reports.RecordManagers;
using HappyTravel.Edo.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public class ReportService : IReportService, IDisposable
    {
        public ReportService(EdoContext context, IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result<Stream>> GetPayableToSupplierReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var to = endDate.Date.AddDays(1);
            
            return await Validate(from, to)
                .Map(GetRecords)
                .Bind(Generate<PayableToSupplierRecordData, PayableToSupplierReportRow>);


            Task<IEnumerable<PayableToSupplierRecordData>> GetRecords()
                => GetRecords<PayableToSupplierRecordData>(from, to);
        }


        public async Task<Result<Stream>> GetAgencyWiseReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var to = endDate.Date.AddDays(1);
            
            return await Validate(from, to)
                .Map(GetRecords)
                .Bind(Generate<AgencyWiseRecordData, AgencyWiseReportRow>);
            
            
            Task<IEnumerable<AgencyWiseRecordData>> GetRecords()
                => GetRecords<AgencyWiseRecordData>(from, to);
        }


        public async Task<Result<Stream>> GetFullBookingsReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var to = endDate.Date.AddDays(1);

            return await Validate(from, to)
                .Map(GetRecords)
                .Bind(Generate<FullBookingsReportData, FullBookingsReportRow>);


            Task<IEnumerable<FullBookingsReportData>> GetRecords()
                => GetRecords<FullBookingsReportData>(from, to);
        }


        public async Task<Result<Stream>> GetFinalizedBookingsReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var to = endDate.Date.AddDays(1);

            return await Validate(from, to)
                .Ensure(DatesAreNotInFuture, "Cannot query future dates for this report")
                .Map(GetRecords)
                .Bind(Generate<FinalizedBookingsReportData, FinalizedBookingsReportRow>);


            Task<IEnumerable<FinalizedBookingsReportData>> GetRecords()
                => GetRecords<FinalizedBookingsReportData>(from, to);


            bool DatesAreNotInFuture()
            {
                var now = _dateTimeProvider.UtcNow();
                return now.Date > endDate.Date;
            }
        }


        public async Task<Result<Stream>> AgenciesProductivityReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var to = endDate.Date.AddDays(1);
            
            return await Validate(from, to)
                .Map(GetRecords)
                .Bind(Generate);


            Task<IEnumerable<AgencyProductivity>> GetRecords()
                => GetRecords<AgencyProductivity>(from, to);
        }


        public Task<Result<Stream>> PendingSupplierReferenceReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate<PendingSupplierReferenceData, PendingSupplierReferenceRow>);


            Task<IEnumerable<PendingSupplierReferenceData>> GetRecords() 
                => GetRecords<PendingSupplierReferenceData>(from, end);
        }

        
        public Task<Result<Stream>> ConfirmedBookingsReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate<ConfirmedBookingsData, ConfirmedBookingsRow>);


            Task<IEnumerable<ConfirmedBookingsData>> GetRecords() 
                => GetRecords<ConfirmedBookingsData>(from, end);
        }
        

        public Task<Result<Stream>> GetHotelWiseBookingReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate<HotelWiseData, HotelWiseRow>);


            Task<IEnumerable<HotelWiseData>> GetRecords() 
                => GetRecords<HotelWiseData>(from, end);
        }
        
        
        public Task<Result<Stream>> GetCancellationDeadlineReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate);


            Task<IEnumerable<CancellationDeadlineData>> GetRecords() 
                => GetRecords<CancellationDeadlineData>(from, end);
        }
        
        
        public Task<Result<Stream>> GetThirdPartySuppliersReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate);


            Task<IEnumerable<ThirdPartySupplierData>> GetRecords() 
                => GetRecords<ThirdPartySupplierData>(from, end);
        }


        public Task<Result<Stream>> GetVccBookingReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);
            
            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate);
            
            
            Task<IEnumerable<VccBookingData>> GetRecords() 
                => GetRecords<VccBookingData>(from, end);
        }


        public Task<Result<Stream>> GetAgentWiseReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);
            
            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate<AgentWiseReportData, AgentWiseReportRow>);
            
            
            Task<IEnumerable<AgentWiseReportData>> GetRecords() 
                => GetRecords<AgentWiseReportData>(from, end);
        }


        public Task<Result<Stream>> GetHotelProductivityReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);
            
            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate);
            
            
            Task<IEnumerable<HotelProductivityData>> GetRecords() 
                => GetRecords<HotelProductivityData>(from, end);
        }


        
        
        public Task<Result<Stream>> GetCancelledBookingsReport(DateTime fromDate, DateTime endDate)
        {
            var from = fromDate.Date;
            var end = endDate.Date.AddDays(1);

            return Result.Success()
                .Map(GetRecords)
                .Bind(Generate);


            Task<IEnumerable<CancelledBookingsReportData>> GetRecords() 
                => GetRecords<CancelledBookingsReportData>(from, end);
        }
       
        
        private Result Validate(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == default || toDate == default)
                return Result.Failure("Range dates required");
            
            if ((fromDate - toDate).TotalDays > MaxDaysInReport)
                return Result.Failure<Stream>($"The interval for generating a report should not exceed {MaxDaysInReport} days");

            return Result.Success();
        }
        
        
        private Task<IEnumerable<TData>> GetRecords<TData>(DateTime fromDate, DateTime toDate) 
            => _serviceProvider.GetRequiredService<IRecordManager<TData>>().Get(fromDate, toDate);


        private async Task<Result<Stream>> Generate<TData, TRow>(IEnumerable<TData> records)
        {
            var stream = await Initialize<TRow>();
            var converter = _serviceProvider.GetRequiredService<IConverter<TData, TRow>>();

            foreach (var record in records)
            {
                var row = converter.Convert(record);
                await Write(row);
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        
        
        private async Task<Result<Stream>> Generate<TRow>(IEnumerable<TRow> records)
        {
            var stream = await Initialize<TRow>();

            foreach (var record in records)
            {
                await Write(record);
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }


        private async Task<Stream> Initialize<TRow>()
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);
            
            _csvWriter.WriteHeader<TRow>();
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();
            
            return stream;
        }


        private async Task Write<TRow>(TRow record)
        {
            _csvWriter.WriteRecord(record);
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();
        }


        private const int MaxDaysInReport = 62;
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;
        
        private readonly EdoContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDateTimeProvider _dateTimeProvider;


        public void Dispose()
        {
            _csvWriter?.Dispose();
            _streamWriter?.Dispose();
            _context?.Dispose();
        }
    }
}