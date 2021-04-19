using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Api.Services.Reports.Converters;
using HappyTravel.Edo.Api.Services.Reports.RecordManagers;
using HappyTravel.Edo.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public class DirectConnectivityReportService : IDirectConnectivityReportService, IDisposable
    {
        public DirectConnectivityReportService(EdoContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }


        public async Task<Result<Stream>> GetSupplierWiseReport(DateTime fromDate, DateTime endDate)
        {
            return await Validate(fromDate, endDate)
                .Map(GetRecords)
                .Bind(Generate<SupplierWiseRecordProjection, SupplierWiseReportRow>);
            
            
            IQueryable<SupplierWiseRecordProjection> GetRecords()
                => GetRecords<SupplierWiseRecordProjection>(fromDate, endDate);
        }


        public async Task<Result<Stream>> GetAgencyWiseReport(DateTime fromDate, DateTime endDate)
        {
            return await Validate(fromDate, endDate)
                .Map(GetRecords)
                .Bind(Generate<AgencyWiseRecordProjection, AgencyWiseReportRow>);
            
            
            IQueryable<AgencyWiseRecordProjection> GetRecords()
                => GetRecords<AgencyWiseRecordProjection>(fromDate, endDate);
        }


        public async Task<Result<Stream>> GetFullBookingsReport(DateTime fromDate, DateTime endDate)
        {
            return await Validate(fromDate, endDate)
                .Map(GetRecords)
                .Bind(Generate<FullBookingsReportProjection, FullBookingsReportRow>);
            
            
            IQueryable<FullBookingsReportProjection> GetRecords()
                => GetRecords<FullBookingsReportProjection>(fromDate, endDate);
        }


        private Result Validate(DateTime fromDate, DateTime endDate)
        {
            if (fromDate == default || endDate == default)
                return Result.Failure("Range dates required");
            
            if ((fromDate - endDate).TotalDays > MaxDaysInReport)
                return Result.Failure("Permissible interval exceeded");

            return Result.Success();
        }
        
        
        private IQueryable<TProjection> GetRecords<TProjection>(DateTime fromDate, DateTime endDate) 
            => _serviceProvider.GetRequiredService<IRecordManager<TProjection>>().Get(fromDate, endDate);


        private async Task<Result<Stream>> Generate<TProjection, TRow>(IEnumerable<TProjection> records)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);
            var converter = _serviceProvider.GetRequiredService<IConverter<TProjection, TRow>>();
            
            _csvWriter.WriteHeader<TRow>();
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();

            foreach (var record in records)
            {
                var row = converter.Convert(record, VatAmount, AmountExcludedVat);
                _csvWriter.WriteRecord(row);
                await _csvWriter.NextRecordAsync();
                await _streamWriter.FlushAsync();
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }


        private static decimal VatAmount(decimal totalAmount) 
            => totalAmount * Vat / (100 + Vat);


        private static decimal AmountExcludedVat(decimal totalAmount) 
            => totalAmount / (1m + Vat / 100m);


        private const int Vat = 5;
        private const int MaxDaysInReport = 62;
        private CsvWriter _csvWriter;
        private StreamWriter _streamWriter;
        
        private readonly EdoContext _context;
        private readonly IServiceProvider _serviceProvider;


        public void Dispose()
        {
            _csvWriter?.Dispose();
            _streamWriter?.Dispose();
            _context?.Dispose();
        }
    }
}