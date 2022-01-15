using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class AgentWiseRecordManager : IRecordManager<AgentWiseReportData>
    {
        public AgentWiseRecordManager(EdoContext context, IAgentContextService agentContext)
        {
            _context = context;
            _agentContext = agentContext;
        }
        
        
        public async Task<IEnumerable<AgentWiseReportData>> Get(DateTime fromDate, DateTime endDate)
        {
            var currentAgent = await _agentContext.GetAgent();
            var bookings = from booking in _context.Bookings
                where booking.AgentId == currentAgent.AgentId && booking.Created >= fromDate && booking.Created < endDate
                select new AgentWiseReportData
                {
                    Created = booking.Created.DateTime,
                    ReferenceCode = booking.ReferenceCode,
                    PaymentMethod = booking.PaymentType,
                    AccommodationName = booking.AccommodationName,
                    Rooms = booking.Rooms,
                    GuestName = booking.MainPassengerName,
                    ArrivalDate = booking.CheckInDate.DateTime,
                    DepartureDate = booking.CheckOutDate.DateTime,
                    TotalPrice = booking.TotalPrice
                };
            
            return await bookings.ToListAsync();
        }


        private readonly EdoContext _context;
        private readonly IAgentContextService _agentContext;
    }
}