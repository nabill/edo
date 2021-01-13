using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class AgentBookingManagementService : IAgentBookingManagementService
    {
        public AgentBookingManagementService(IBookingManagementService managementService, IBookingRecordsManager recordsManager)
        {
            _managementService = managementService;
            _recordsManager = recordsManager;
        }


        public async Task<Result> Cancel(int bookingId, AgentContext agent)
        {
            return await GetBooking(bookingId, agent)
                .Bind(Cancel);

            
            Task<Result> Cancel(Booking booking) 
                => _managementService.Cancel(booking, agent.ToUserInfo());
        }

        
        public async Task<Result> RefreshStatus(int bookingId, AgentContext agent)
        {
            return await GetBooking(bookingId, agent)
                .Bind(Refresh);

            
            Task<Result> Refresh(Booking booking) 
                => _managementService.RefreshStatus(booking, agent.ToUserInfo());
        }


        private Task<Result<Booking>> GetBooking(int bookingId, AgentContext agent) 
            => _recordsManager.Get(bookingId, agent.AgentId);

        
        private readonly IBookingManagementService _managementService;
        private readonly IBookingRecordsManager _recordsManager;
    }
}