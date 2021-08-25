using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class AgencyWiseRecordManager : IRecordManager<AgencyWiseRecordData>
    {
        public AgencyWiseRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<AgencyWiseRecordData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                where
                    booking.IsDirectContract &&
                    booking.Created >= fromDate &&
                    booking.Created < endDate
                select new AgencyWiseRecordData
                {
                    Date = booking.Created,
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
                    PaymentStatus = booking.PaymentStatus
                })
                .ToListAsync();
        }
        
        
        private readonly EdoContext _context;
    }
}