using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Formatters;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public class DirectConnectivityReportService : IDirectConnectivityReportService, IDisposable
    {
        public DirectConnectivityReportService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<Stream>> GetSupplierWiseReport(Suppliers supplier, DateTime dateFrom, DateTime dateEnd)
        {
            if (dateFrom == default || dateEnd == default)
                return Result.Failure<Stream>("Range dates required");
            
            if ((dateEnd - dateFrom).TotalDays > MaxRange)
                return Result.Failure<Stream>("Permissible interval exceeded");

            var query = from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                where 
                    booking.SystemTags.Contains(EdoContracts.Accommodations.Constants.CommonTags.DirectConnectivity) &&
                    booking.Created >= dateFrom &&
                    booking.Created < dateEnd &&
                    booking.Supplier == supplier
                select new SupplierWiseRecordProjection
                {
                    ReferenceCode = booking.ReferenceCode,
                    InvoiceNumber = invoice.Number,
                    HotelName = booking.AccommodationName,
                    HotelConfirmationNumber = booking.SupplierReferenceCode,
                    Rooms = booking.Rooms,
                    GuestName = booking.MainPassengerName,
                    ArrivalDate = booking.CheckInDate,
                    DepartureDate = booking.CheckOutDate,
                    TotalAmount = order.Price
                };

            return await Generate(query);
        }
        
        
        private async Task<Stream> Generate(IEnumerable<SupplierWiseRecordProjection> records)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);

            _csvWriter.WriteHeader<SupplierWiseReportRow>();
            await _csvWriter.NextRecordAsync();
            foreach (var record in records)
            {
                var row = new SupplierWiseReportRow
                {
                    ReferenceCode = record.ReferenceCode,
                    InvoiceNumber = record.InvoiceNumber,
                    HotelName = record.HotelName,
                    HotelConfirmationNumber = record.HotelConfirmationNumber,
                    RoomTypes = string.Join("; ", record.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                    GuestName = record.GuestName ?? string.Empty,
                    ArrivalDate = DateTimeFormatters.ToDateString(record.ArrivalDate),
                    DepartureDate = DateTimeFormatters.ToDateString(record.DepartureDate),
                    LenghtOfStay = (record.DepartureDate - record.ArrivalDate).TotalDays,
                    AmountExclVat = AmountExcludedVat(record.TotalAmount),
                    VatAmount = VatAmount(record.TotalAmount),
                    TotalAmount = record.TotalAmount
                };
                
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
            return totalAmount / (1 + Vat / 100);
        }

        
        private const int Vat = 5;
        private const int MaxRange = 31;
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