using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class BookingInfoService
    {
        public BookingInfoService(IBookingInfoService bookingInfoService)
        {
            _bookingInfoService = bookingInfoService;
        }


        public async Task<Result<Booking>> Get(string referenceCode, AgentContext agent)
        {
            var (isSuccess, _, booking, error) = await _bookingInfoService.GetAgentsBooking(referenceCode, agent);

            return isSuccess
                ? booking.FromEdoModels()
                : Result.Failure<Booking>(error);
        }


        private readonly IBookingInfoService _bookingInfoService;
    }
}