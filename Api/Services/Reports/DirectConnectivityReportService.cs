using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Models.Reports.Converters;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
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


        public async Task<Result<Stream>> GetSupplierWiseReport(DateTime fromDate, DateTime endTime)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate<SupplierWiseRecordProjection, SupplierWiseReportRow>);


            Result Validate() 
                => ValidateDates(fromDate, endTime);


            IQueryable<SupplierWiseRecordProjection> GetRecords()
            {
                return from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    where 
                        booking.IsDirectContract &&
                        booking.Created >= fromDate &&
                        booking.Created < endTime
                    select new SupplierWiseRecordProjection
                    {
                        ReferenceCode = booking.ReferenceCode,
                        InvoiceNumber = invoice.Number,
                        AccommodationName = booking.AccommodationName,
                        ConfirmationNumber = booking.SupplierReferenceCode,
                        Rooms = booking.Rooms,
                        GuestName = booking.MainPassengerName,
                        ArrivalDate = booking.CheckInDate,
                        DepartureDate = booking.CheckOutDate,
                        OriginalAmount = order.OriginalSupplierPrice,
                        OriginalCurrency = order.OriginalSupplierCurrency,
                        ConvertedAmount = order.ConvertedSupplierPrice,
                        ConvertedCurrency = order.ConvertedSupplierCurrency,
                        Supplier = booking.Supplier
                    };
            }
        }


        public async Task<Result<Stream>> GetAgencyWiseReport(DateTime fromDate, DateTime endTime)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate<AgencyWiseRecordProjection, AgencyWiseReportRow>);


            Result Validate() 
                => ValidateDates(fromDate, endTime);
            
            
            IQueryable<AgencyWiseRecordProjection> GetRecords()
            {
                return from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    join agency in _context.Agencies on booking.AgencyId equals agency.Id
                    where 
                        booking.IsDirectContract &&
                        booking.Created >= fromDate &&
                        booking.Created < endTime
                    select new AgencyWiseRecordProjection
                    {
                        Date = booking.Created,
                        ReferenceCode = booking.ReferenceCode,
                        InvoiceNumber = invoice.Number,
                        AgencyName = agency.Name,
                        PaymentMethod = booking.PaymentMethod,
                        AccommodationName = booking.AccommodationName,
                        ConfirmationNumber = booking.SupplierReferenceCode,
                        Rooms = booking.Rooms,
                        GuestName = booking.MainPassengerName,
                        ArrivalDate = booking.CheckInDate,
                        DepartureDate = booking.CheckOutDate,
                        OriginalAmount = order.OriginalSupplierPrice,
                        OriginalCurrency = order.OriginalSupplierCurrency,
                        ConvertedAmount = order.ConvertedSupplierPrice,
                        ConvertedCurrency = order.ConvertedSupplierCurrency,
                        PaymentStatus = booking.PaymentStatus
                    };
            }
        }


        public async Task<Result<Stream>> GetFullBookingsReport(DateTime fromDate, DateTime endTime)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate<FullBookingsReportProjection, FullBookingsReportRow>);
            
            
            Result Validate() 
                => ValidateDates(fromDate, endTime);
            
            
            IQueryable<FullBookingsReportProjection> GetRecords()
            {
                return from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    join agency in _context.Agencies on booking.AgencyId equals agency.Id
                    where
                        booking.CheckOutDate >= fromDate &&
                        booking.CheckOutDate < endTime
                    select new FullBookingsReportProjection
                    {
                        Created = booking.Created,
                        ReferenceCode = booking.ReferenceCode,
                        InvoiceNumber = invoice.Number,
                        AgencyName = agency.Name,
                        PaymentMethod = booking.PaymentMethod,
                        AccommodationName = booking.AccommodationName,
                        ConfirmationNumber = booking.SupplierReferenceCode,
                        Rooms = booking.Rooms,
                        GuestName = booking.MainPassengerName,
                        ArrivalDate = booking.CheckInDate,
                        DepartureDate = booking.CheckOutDate,
                        OriginalAmount = order.OriginalSupplierPrice,
                        OriginalCurrency = order.OriginalSupplierCurrency,
                        ConvertedAmount = order.ConvertedSupplierPrice,
                        ConvertedCurrency = order.ConvertedSupplierCurrency,
                        PaymentStatus = booking.PaymentStatus,
                        Supplier = booking.Supplier
                    };
            }
        }


        private Result ValidateDates(DateTime dateFrom, DateTime dateEnd)
        {
            if (dateFrom == default || dateEnd == default)
                return Result.Failure("Range dates required");
            
            if ((dateEnd - dateFrom).TotalDays > MaxDaysInReport)
                return Result.Failure("Permissible interval exceeded");

            return Result.Success();
        }


        private async Task<Result<Stream>> Generate<TProjection, TRow>(IEnumerable<TProjection> records)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);
            var mapper = _serviceProvider.GetRequiredService<IConverter<TProjection, TRow>>();
            
            _csvWriter.WriteHeader<TRow>();
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();

            foreach (var record in records)
            {
                var row = mapper.Convert(record, VatAmount, AmountExcludedVat);
                _csvWriter.WriteRecord(row);
                await _csvWriter.NextRecordAsync();
                await _streamWriter.FlushAsync();
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }


        private static decimal VatAmount(decimal totalAmount)
        {
            return totalAmount * Vat / (100 + Vat);
        }


        private static decimal AmountExcludedVat(decimal totalAmount)
        {
            return totalAmount / (1m + Vat / 100m);
        }


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