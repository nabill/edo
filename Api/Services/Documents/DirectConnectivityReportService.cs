using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using CsvHelper;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
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


        public async Task<Result<Stream>> GetSupplierWiseReport(DateTime dateFrom, DateTime dateEnd)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate<SupplierWiseRecordProjection, SupplierWiseReportRow>);


            Result Validate()
            {
                if (dateFrom == default || dateEnd == default)
                    return Result.Failure<Stream>("Range dates required");
            
                if ((dateEnd - dateFrom).TotalDays > MaxRange)
                    return Result.Failure<Stream>("Permissible interval exceeded");

                return Result.Success();
            }


            IQueryable<SupplierWiseRecordProjection> GetRecords()
            {
                return from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    where 
                        booking.Tags.Contains(EdoContracts.Accommodations.Constants.CommonTags.DirectConnectivity) &&
                        booking.Created >= dateFrom &&
                        booking.Created < dateEnd
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
                        TotalAmount = order.Price,
                        Supplier = booking.Supplier
                    };
            }
        }


        public async Task<Result<Stream>> GetAgencyWiseReport(DateTime dateFrom, DateTime dateEnd)
        {
            return await Validate()
                .Map(GetRecords)
                .Bind(Generate<AgencyWiseRecordProjection, AgencyWiseReportRow>);


            async Task<Result> Validate()
            {
                if (dateFrom == default || dateEnd == default)
                    return Result.Failure<Stream>("Range dates required");
            
                if ((dateEnd - dateFrom).TotalDays > MaxRange)
                    return Result.Failure<Stream>("Permissible interval exceeded");

                return Result.Success();
            }
            
            
            IQueryable<AgencyWiseRecordProjection> GetRecords()
            {
                return from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    join agency in _context.Agencies on booking.AgencyId equals agency.Id
                    where 
                        booking.SystemTags.Contains(EdoContracts.Accommodations.Constants.CommonTags.DirectConnectivity) &&
                        booking.Created >= dateFrom &&
                        booking.Created < dateEnd
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
                        TotalAmount = order.Price,
                        PaymentStatus = booking.PaymentStatus
                    };
            }
        }


        private async Task<Result<Stream>> Generate<TProjection, TRow>(IEnumerable<TProjection> records)
        {
            var stream = new MemoryStream();
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, CultureInfo.InvariantCulture);
            
            _csvWriter.WriteHeader<TRow>();
            await _csvWriter.NextRecordAsync();
            await _streamWriter.FlushAsync();
            
            foreach (var record in records)
            {
                var (_, isFailure, row, error) = Map(record);
                if (isFailure)
                    return Result.Failure<Stream>(error);

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


        private static Result<object> Map<T>(T entity)
        {
            return entity switch
            {
                SupplierWiseRecordProjection e => new SupplierWiseReportRow
                {
                    ReferenceCode = e.ReferenceCode,
                    InvoiceNumber = e.InvoiceNumber,
                    AccommodationName = e.AccommodationName,
                    ConfirmationNumber = e.ConfirmationNumber,
                    RoomTypes = string.Join("; ", e.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                    GuestName = e.GuestName ?? string.Empty,
                    ArrivalDate = DateTimeFormatters.ToDateString(e.ArrivalDate),
                    DepartureDate = DateTimeFormatters.ToDateString(e.DepartureDate),
                    LenghtOfStay = (e.DepartureDate - e.ArrivalDate).TotalDays,
                    AmountExclVat = Math.Round(AmountExcludedVat(e.TotalAmount), 2),
                    VatAmount = Math.Round(VatAmount(e.TotalAmount), 2),
                    TotalAmount = e.TotalAmount,
                    Supplier = EnumFormatters.FromDescription(e.Supplier)
                },
                AgencyWiseRecordProjection e => new AgencyWiseReportRow
                {
                    Date = DateTimeFormatters.ToDateString(e.Date),
                    ReferenceCode = e.ReferenceCode,
                    InvoiceNumber = e.InvoiceNumber,
                    AgencyName = e.AgencyName,
                    PaymentMethod = EnumFormatters.FromDescription(e.PaymentMethod),
                    GuestName = e.GuestName,
                    AccommodationName = e.AccommodationName,
                    Rooms = string.Join("; ", e.Rooms.Select(r => EnumFormatters.FromDescription(r.Type))),
                    ArrivalDate = DateTimeFormatters.ToDateString(e.ArrivalDate),
                    DepartureDate = DateTimeFormatters.ToDateString(e.DepartureDate),
                    LenghtOfStay = (e.DepartureDate - e.ArrivalDate).TotalDays,
                    TotalAmount = e.TotalAmount,
                    ConfirmationNumber = e.ConfirmationNumber,
                    PaymentStatus = EnumFormatters.FromDescription(e.PaymentStatus)
                },
                _ => Result.Failure<object>($"Type {typeof(T)} is not supported")
            };
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