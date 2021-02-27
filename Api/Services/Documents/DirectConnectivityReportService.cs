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
        
        
        private async Task<Stream> Generate(IEnumerable<SupplierWiseRecordProjection> rows)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);

            _csvWriter.WriteHeader<SupplierWiseReportLine>();
            await _csvWriter.NextRecordAsync();
            foreach (var row in rows)
            {
                var line = new SupplierWiseReportLine
                {
                    ReferenceCode = row.ReferenceCode,
                    InvoiceNumber = row.InvoiceNumber,
                    HotelName = row.HotelName,
                    HotelConfirmationNumber = row.HotelConfirmationNumber,
                    RoomTypes = string.Join("; ", row.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                    GuestName = row.GuestName ?? string.Empty,
                    ArrivalDate = DateTimeFormatters.ToDateString(row.ArrivalDate),
                    DepartureDate = DateTimeFormatters.ToDateString(row.DepartureDate),
                    LenghtOfStay = (row.DepartureDate - row.ArrivalDate).TotalDays,
                    AmountExclVat = AmountExcludedVat(row.TotalAmount),
                    VatAmount = VatAmount(row.TotalAmount),
                    TotalAmount = row.TotalAmount
                };
                
                _csvWriter.WriteRecord(line);
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