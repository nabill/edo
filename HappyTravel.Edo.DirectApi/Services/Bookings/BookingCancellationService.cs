using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingCancellationService
    {
        public BookingCancellationService(IAgentBookingManagementService bookingManagementService)
        {
            _bookingManagementService = bookingManagementService;
        }


        public Task<Result> Cancel(string referenceCode, AgentContext agent) 
            => _bookingManagementService.Cancel(referenceCode, agent);


        private readonly IAgentBookingManagementService _bookingManagementService;
    }
}