using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class VccBookingRecordManager : IRecordManager<VccBookingData>
    {
        public VccBookingRecordManager(EdoContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }
        
        
        public async Task<IEnumerable<VccBookingData>> Get(DateTime fromDate, DateTime endDate)
        {
            var bookings = await _context.Bookings
                .Where(b => b.Created >= fromDate && b.Created < endDate)
                .ToListAsync();

            if (!bookings.Any())
                return new List<VccBookingData>(0);

            var referenceCodes = bookings.Select(b => b.ReferenceCode).ToList();
            var client = _clientFactory.CreateClient(HttpClientNames.VccApi);
            var vccBookingsInfoResponse = await client.PostAsJsonAsync("/api/1.0/history", referenceCodes);
            
            if (!vccBookingsInfoResponse.IsSuccessStatusCode)
                return new List<VccBookingData>(0);

            var vccBookingsInfo = await vccBookingsInfoResponse.Content.ReadFromJsonAsync<List<VccBookingInfo>>();
            if (vccBookingsInfo is null || !vccBookingsInfo.Any())
                return  new List<VccBookingData>(0);

            return bookings
                .Join(vccBookingsInfo, booking => booking.ReferenceCode, info => info.ReferenceCode, (booking, info) => new { Booking = booking, Vcc = info })
                .Select(x => new VccBookingData
                {
                    GuestName = x.Booking.MainPassengerName,
                    ReferenceCode = x.Booking.ReferenceCode,
                    CheckingDate = x.Booking.CheckInDate,
                    CheckOutDate = x.Booking.CheckOutDate,
                    Amount = x.Booking.TotalPrice,
                    Currency = x.Booking.Currency,
                    CardActivationDate = x.Vcc.ActivationDate,
                    CardDueDate = x.Vcc.DueDate,
                    CardAmount = x.Vcc.Amount,
                    CardNumber = x.Vcc.CardNumber
                })
                .AsQueryable();
        }
        
        
        private readonly EdoContext _context;
        private readonly IHttpClientFactory _clientFactory;
    }
}