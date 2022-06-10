using System.Linq;
using System.Threading.Tasks;
using Api.Infrastructure.Options;
using Api.Models.Bookings;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;
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
            _bookingInfoService = bookingInfoService;
            _agentContext = agentContext;
            _contractKindCommissionOptions = contractKindCommissionOptions.Value;
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


        public async Task<Result<AccommodationBookingInfo>> RecalculatePrice(string referenceCode,
            BookingRecalculatePriceRequest request, string languageCode)
        {
            var agent = await _agentContext.GetAgent();
            
            return await GetBooking(referenceCode, agent)
                .Map(Recalculate)
                .Tap(UpdateBooking)
                .Bind(ToBookingInfo);


            Booking Recalculate(Booking booking)
            {
                var commission = 0m;

                switch (request.PaymentType)
                {
                    case PaymentTypes.CreditCard:
                        commission = _contractKindCommissionOptions.CreditCardPaymentsCommission;
                        break;
                }

                var netPrice = new MoneyAmount(booking.NetPrice, booking.Currency);
                var contractFinalPrice = MoneyRounder.Ceil(netPrice.ApplyCommission(commission));
                var roomFinalPrices = booking.Rooms
                    .Select(r => MoneyRounder.Ceil(r.NetPrice.ApplyCommission(commission)))
                    .ToList();

                var (alignedFinalPrice, alignedRoomFinalPrices) = PriceAligner.AlignAggregatedValues(contractFinalPrice, roomFinalPrices);

                for (var i = 0; i < booking.Rooms.Count; i++)
                {

                    var room = booking.Rooms[i];
                    room.Price = alignedRoomFinalPrices[i];
                    room.Commission = commission;
                }

                booking.TotalPrice = alignedFinalPrice.Amount;
                booking.Commission = commission;
                booking.PaymentType = request.PaymentType;

                return booking;
            }


            async Task<Booking> UpdateBooking(Booking booking)
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();

                return booking;
            }


            Task<Result<AccommodationBookingInfo>> ToBookingInfo(Booking booking)
                => _bookingInfoService.ConvertToBookingInfo(booking, languageCode, agent);
        }


        private Task<Result<Booking>> GetBooking(int bookingId, AgentContext agent)
            => _recordManager.Get(bookingId).CheckPermissions(agent);


        private Task<Result<Booking>> GetBooking(string referenceCode, AgentContext agent)
            => _recordManager.Get(referenceCode).CheckPermissions(agent);


        private readonly EdoContext _context;
        private readonly ISupplierBookingManagementService _managementService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingStatusRefreshService _statusRefreshService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly ContractKindCommissionOptions _contractKindCommissionOptions;
        private readonly IAgentContextService _agentContext;
    }
}