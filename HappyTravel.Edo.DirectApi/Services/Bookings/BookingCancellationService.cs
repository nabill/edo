using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingCancellationService
    {
        public BookingCancellationService(IAgentBookingManagementService bookingManagementService,
            BookingInfoService bookingInfoService)
        {
            _bookingManagementService = bookingManagementService;
            _bookingInfoService = bookingInfoService;
        }


        public async Task<Result<Booking>> Cancel(string clientReferenceCode, AgentContext agent)
        {
            var booking = await _bookingInfoService.Get(clientReferenceCode, agent);
            if (booking.IsFailure)
                return Result.Failure<Booking>(booking.Error);
            
            var (_, isFailure, error) = await _bookingManagementService.Cancel(booking.Value.ReferenceCode, agent);
            if (isFailure)
                return Result.Failure<Booking>(error);
            
            var refreshedBooking = await _bookingInfoService.Get(clientReferenceCode, agent);
            return refreshedBooking.Value.FromEdoModels();
        }


        private readonly IAgentBookingManagementService _bookingManagementService;
        private readonly BookingInfoService _bookingInfoService;
    }
}