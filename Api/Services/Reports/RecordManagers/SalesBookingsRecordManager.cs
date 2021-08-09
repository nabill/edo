using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class SalesBookingsRecordManager : IRecordManager<SalesBookingsReportData>
    {
        public SalesBookingsRecordManager(EdoContext context,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }
        
        
        public async Task<IEnumerable<SalesBookingsReportData>> Get(DateTime fromDate, DateTime endDate)
        {
            var data = await (from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join supplier in _context.SupplierOrders on booking.ReferenceCode equals supplier.ReferenceCode
                let cancellationDate = _context.BookingStatusHistory
                    .Where(c => c.BookingId == booking.Id && c.Status == BookingStatuses.Cancelled)
                    .Select(c => c.CreatedAt)
                    .FirstOrDefault()
                where
                    (booking.CheckOutDate >= fromDate &&
                    booking.CheckOutDate < endDate
                    ||
                    booking.Created >= fromDate &&
                    booking.Created < endDate)
                    &&
                    (booking.Status == BookingStatuses.Confirmed ||
                    booking.Status == BookingStatuses.Cancelled && cancellationDate >= booking.DeadlineDate)
                    &&
                    (order.ConvertedPrice > 0m || booking.TotalPrice > 0m)
                select new SalesBookingsReportData
                {
                    Created = booking.Created,
                    ReferenceCode = booking.ReferenceCode,
                    BookingStatus = booking.Status,
                    InvoiceNumber = invoice.Number,
                    AgencyName = agency.Name,
                    PaymentMethod = booking.PaymentType,
                    AccommodationName = booking.AccommodationName,
                    ConfirmationNumber = booking.SupplierReferenceCode,
                    Rooms = booking.Rooms,
                    GuestName = booking.MainPassengerName,
                    ArrivalDate = booking.CheckInDate,
                    DepartureDate = booking.CheckOutDate,
                    OrderAmount = order.Price,
                    OrderCurrency = order.Currency,
                    TotalPrice = booking.TotalPrice,
                    TotalCurrency = booking.Currency,
                    ConvertedAmount = order.ConvertedPrice,
                    ConvertedCurrency = order.ConvertedCurrency,
                    PaymentStatus = booking.PaymentStatus,
                    Supplier = booking.Supplier,
                    CancellationPolicies = booking.CancellationPolicies,
                    CancellationDate = cancellationDate,
                    IsDirectContract = booking.IsDirectContract,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    ServiceDeadline = booking.DeadlineDate,
                    SupplierDeadline = supplier.Deadline.Date
                })
                .ToListAsync();

            var now = _dateTimeProvider.UtcNow();
            return data.Where(p =>
                {
                    var nonRefundableDate = GetNonRefundableDate(p);
                    return nonRefundableDate > now &&
                        p.CheckOutDate >= fromDate &&
                        p.CheckOutDate < endDate
                        ||
                        nonRefundableDate <= now &&
                        p.Created >= fromDate &&
                        p.Created < endDate;
                });
        }


        private DateTime GetNonRefundableDate(SalesBookingsReportData data)
        {
            var lastPolicy = data.CancellationPolicies
                .OrderBy(p => p.FromDate)
                .FirstOrDefault(p => p.Percentage >= 100d);
            return lastPolicy?.FromDate ?? data.CheckInDate;
        }
        
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}