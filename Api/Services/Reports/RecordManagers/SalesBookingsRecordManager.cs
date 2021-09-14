using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

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
            var bookings = await (from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                join supplierOrder in _context.SupplierOrders on booking.ReferenceCode equals supplierOrder.ReferenceCode
                where (booking.Status == BookingStatuses.Confirmed ||
                        booking.Status == BookingStatuses.Cancelled && booking.Cancelled >= booking.DeadlineDate)
                    && (booking.Created >= fromDate && booking.Created < endDate
                        || booking.CheckOutDate >= fromDate && booking.CheckOutDate < endDate)
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
                    CancellationDate = booking.Cancelled,
                    IsDirectContract = booking.IsDirectContract,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    AgentDeadline = booking.DeadlineDate,
                    SupplierDeadline = supplierOrder.Deadline
                })
                .ToListAsync();
            
            // ! We need to filter by CheckOutDate and Created second time to get the correct results !
            
            var notAdvancedPurchaseBookings = bookings
                .Where(booking => booking.Rooms.All(room => !room.IsAdvancePurchaseRate))
                .Where(booking => booking.CheckOutDate >= fromDate && booking.CheckOutDate < endDate);

            var advancedPurchaseBookings = bookings
                .Where(booking => booking.Rooms.Any(room => room.IsAdvancePurchaseRate))
                .Where(booking => booking.Created >= fromDate && booking.Created < endDate);

            return advancedPurchaseBookings.Union(notAdvancedPurchaseBookings);
        }


        private readonly EdoContext _context;
    }
}