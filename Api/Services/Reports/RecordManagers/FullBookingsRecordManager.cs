using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports.DirectConnectivityReports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class FullBookingsRecordManager : IRecordManager<FullBookingsReportData>
    {
        public FullBookingsRecordManager(EdoContext context)
        {
            _context = context;
        }
        
        
        public async Task<IEnumerable<FullBookingsReportData>> Get(DateTime fromDate, DateTime endDate)
        {
            return await (from booking in _context.Bookings
                    join invoice in _context.Invoices on booking.ReferenceCode equals invoice.ParentReferenceCode
                    join order in _context.SupplierOrders on booking.ReferenceCode equals order.ReferenceCode
                    join agency in _context.Agencies on booking.AgencyId equals agency.Id
                    join agent in _context.Agents on booking.AgentId equals agent.Id
                    join country in _context.Countries on agency.CountryCode equals country.Code
                    join market in _context.Markets on country.MarketId equals market.Id
                    where
                        booking.CheckOutDate >= fromDate &&
                        booking.CheckOutDate < endDate
                    select new FullBookingsReportData
                    {
                        Created = booking.Created.DateTime,
                        ReferenceCode = booking.ReferenceCode,
                        Status = booking.Status.ToString(),
                        InvoiceNumber = invoice.Number,
                        AgencyName = agency.Name,
                        AgencyCity = agency.City,
                        AgencyCountry = country.Names,
                        AgentName = $"{agent.FirstName} {agent.LastName}",
                        AgencyMarket = market.Names,
                        PaymentMethod = booking.PaymentType,
                        AccommodationName = booking.AccommodationName,
                        ConfirmationNumber = booking.SupplierReferenceCode,
                        Rooms = booking.Rooms,
                        GuestName = booking.MainPassengerName,
                        ArrivalDate = booking.CheckInDate.DateTime,
                        DepartureDate = booking.CheckOutDate.DateTime,
                        OriginalAmount = order.Price,
                        OriginalCurrency = order.Currency,
                        ConvertedAmount = order.ConvertedPrice,
                        ConvertedCurrency = order.ConvertedCurrency,
                        PaymentStatus = booking.PaymentStatus,
                        SupplierCode = booking.SupplierCode,
                        CancellationDate = booking.Cancelled == null
                            ? null
                            : booking.Cancelled.Value.DateTime
                    })
                .ToListAsync();
        }
        
        
        private readonly EdoContext _context;
    }
}