using System.Threading.Tasks;
using Api.Infrastructure.Options;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class AgentBookingManagementService : IAgentBookingManagementService
    {
        public AgentBookingManagementService(EdoContext context, ISupplierBookingManagementService managementService,
            IBookingRecordManager recordManager, IBookingStatusRefreshService statusRefreshService,
            IBookingInfoService bookingInfoService, IOptions<ContractKindCommissionOptions> contractKindCommissionOptions,
            IAgentContextService agentContext)
        {
            _managementService = managementService;
            _recordManager = recordManager;
            _statusRefreshService = statusRefreshService;
            _agentContext = agentContext;
            _context = context;
        }


        public async Task<Result> Cancel(int bookingId)
        {
            var agent = await _agentContext.GetAgent();

            return await GetBooking(bookingId, agent)
                .Bind(Cancel);


            Task<Result> Cancel(Booking booking)
                => _managementService.Cancel(booking, agent.ToApiCaller(), BookingChangeEvents.Cancel);
        }


        public async Task<Result> Cancel(string referenceCode)
        {
            var agent = await _agentContext.GetAgent();

            return await GetBooking(referenceCode, agent)
                .Bind(Cancel);


            Task<Result> Cancel(Booking booking)
                => _managementService.Cancel(booking, agent.ToApiCaller(), BookingChangeEvents.Cancel);
        }


        public async Task<Result> RefreshStatus(int bookingId)
        {
            var agent = await _agentContext.GetAgent();

            return await GetBooking(bookingId, agent)
                .Bind(Refresh);


            Task<Result> Refresh(Booking booking)
                => _statusRefreshService.RefreshStatus(booking.Id, agent.ToApiCaller());
        }


        private Task<Result<Booking>> GetBooking(int bookingId, AgentContext agent)
            => _recordManager.Get(bookingId).CheckPermissions(agent);


        private Task<Result<Booking>> GetBooking(string referenceCode, AgentContext agent)
            => _recordManager.Get(referenceCode).CheckPermissions(agent);


        private readonly EdoContext _context;
        private readonly ISupplierBookingManagementService _managementService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingStatusRefreshService _statusRefreshService;
        private readonly IAgentContextService _agentContext;
    }
}