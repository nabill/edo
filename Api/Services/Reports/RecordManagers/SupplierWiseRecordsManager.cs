using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class SupplierWiseRecordsManager : IRecordManager<SupplierWiseRecordData>
    {
        public SupplierWiseRecordsManager(EdoContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<SupplierWiseRecordData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                where
                    booking.IsDirectContract &&
                    booking.Created >= fromDate &&
                    booking.Created < endDate
                select new SupplierWiseRecordData
                {
                    ReferenceCode = booking.ReferenceCode,
                    InvoiceNumber = invoice.Number,
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
                    Supplier = booking.Supplier
                })
                .ToListAsync();
        }
        
        
        private readonly EdoContext _context;
    }
}