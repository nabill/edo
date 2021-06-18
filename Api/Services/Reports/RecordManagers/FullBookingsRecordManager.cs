using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class FullBookingsRecordManager : IRecordManager<FullBookingsReportProjection>
    {
        public FullBookingsRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public IQueryable<FullBookingsReportProjection> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                let cancellationDate = _context.BookingStatusHistory
                    .Where(c => c.BookingId == booking.Id && c.Status == BookingStatuses.Cancelled)
                    .Select(c => c.CreatedAt)
                    .FirstOrDefault()
                where
                    booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                select new FullBookingsReportProjection
                {
                    Created = booking.Created,
                    ReferenceCode = booking.ReferenceCode,
                    InvoiceNumber = invoice.Number,
                    AgencyName = agency.Name,
                    PaymentMethod = booking.PaymentType,
                    AccommodationName = booking.AccommodationName,
                    ConfirmationNumber = booking.SupplierReferenceCode,
                    Rooms = booking.Rooms,
                    GuestName = booking.MainPassengerName,
                    ArrivalDate = booking.CheckInDate,
                    DepartureDate = booking.CheckOutDate,
                    OriginalAmount = order.Price,
                    OriginalCurrency = order.Currency,
                    ConvertedAmount = order.ConvertedPrice,
                    ConvertedCurrency = order.ConvertedCurrency,
                    PaymentStatus = booking.PaymentStatus,
                    Supplier = booking.Supplier,
                    CancellationPolicies = booking.CancellationPolicies,
                    CancellationDate = cancellationDate
                };
        }
        
        
        private readonly EdoContext _context;
    }
}