using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class AgentBookingManagementService : IAgentBookingManagementService
    {
        public AgentBookingManagementService(ISupplierBookingManagementService managementService, 
            IBookingRecordManager recordManager, IBookingStatusRefreshService statusRefreshService)
        {
            _managementService = managementService;
            _recordManager = recordManager;
            _statusRefreshService = statusRefreshService;
        }


        public async Task<Result> Cancel(int bookingId, AgentContext agent)
        {
            return await GetBooking(bookingId, agent)
                .Bind(Cancel);

            
            Task<Result> Cancel(Booking booking) 
                => _managementService.Cancel(booking, agent.ToApiCaller(), BookingChangeEvents.Cancel);
        }
        
        
        public async Task<Result> Cancel(string referenceCode, AgentContext agent)
        {
            return await GetBooking(referenceCode, agent)
                .Bind(Cancel);

            
            Task<Result> Cancel(Booking booking) 
                => _managementService.Cancel(booking, agent.ToApiCaller(), BookingChangeEvents.Cancel);
        }

        
        public async Task<Result> RefreshStatus(int bookingId, AgentContext agent)
        {
            return await GetBooking(bookingId, agent)
                .Bind(Refresh);

            
            Task<Result> Refresh(Booking booking) 
                => _statusRefreshService.RefreshStatus(booking.Id, agent.ToApiCaller());
        }


        private Task<Result<Booking>> GetBooking(int bookingId, AgentContext agent) 
            => _recordManager.Get(bookingId).CheckPermissions(agent);
        
        
        private Task<Result<Booking>> GetBooking(string referenceCode, AgentContext agent) 
            => _recordManager.Get(referenceCode).CheckPermissions(agent);

        
        private readonly ISupplierBookingManagementService _managementService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingStatusRefreshService _statusRefreshService;
    }
}