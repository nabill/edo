using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class SalesBookingsRecordManager : IRecordManager<SalesBookingsReportData>
    {
        public SalesBookingsRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<SalesBookingsReportData>> Get(DateTime fromDate, DateTime endDate)
        {
            var bookingsQuery = from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join supplierOrder in _context.SupplierOrders on booking.ReferenceCode equals supplierOrder.ReferenceCode
                let cancellationDate = _context.BookingStatusHistory
                    .Where(c => c.BookingId == booking.Id && c.Status == BookingStatuses.Cancelled)
                    .Select(c => c.CreatedAt)
                    .FirstOrDefault()
                where booking.Status == BookingStatuses.Confirmed ||
                    booking.Status == BookingStatuses.Cancelled && cancellationDate >= booking.DeadlineDate
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
                    SupplierPrice = supplierOrder.Price,
                    SupplierCurrency = supplierOrder.Currency,
                    AgentPrice = booking.TotalPrice,
                    AgentCurrency = booking.Currency,
                    SupplierConvertedPrice = supplierOrder.ConvertedPrice,
                    SupplierConvertedCurrency = supplierOrder.ConvertedCurrency,
                    PaymentStatus = booking.PaymentStatus,
                    Supplier = booking.Supplier,
                    CancellationPolicies = booking.CancellationPolicies,
                    CancellationDate = cancellationDate,
                    IsDirectContract = booking.IsDirectContract,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    AgentDeadline = booking.DeadlineDate,
                    SupplierDeadline = supplierOrder.Deadline
                };

            var notAdvancedPurchaseBookings = bookingsQuery
                .AsEnumerable()
                .Where(booking => booking.Rooms.All(room => !room.IsAdvancePurchaseRate))
                .Where(booking => booking.CheckOutDate >= fromDate && booking.CheckOutDate < endDate)
                .ToList();

            var advancedPurchaseBookings = bookingsQuery
                .AsEnumerable()
                .Where(booking => booking.Rooms.Any(room => room.IsAdvancePurchaseRate))
                .Where(booking => booking.Created >= fromDate && booking.Created < endDate)
                .ToList();

            return advancedPurchaseBookings.Union(notAdvancedPurchaseBookings).ToList();
        }


        private readonly EdoContext _context;
    }
}