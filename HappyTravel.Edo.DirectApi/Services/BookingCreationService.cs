using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class BookingCreationService
    {
        public BookingCreationService(IFinancialAccountBookingFlow bookingFlow)
        {
            _bookingFlow = bookingFlow;
        }


        public async Task<Result<AccommodationBookingInfo>> Book(AccommodationBookingRequest request, AgentContext agent, string languageCode)
        {
            var (isSuccess, _, result, error) = await _bookingFlow.BookByAccount(request.ToEdoModel(), agent, languageCode, string.Empty);

            return isSuccess
                ? result.FromEdoModel()
                : Result.Failure<AccommodationBookingInfo>(error);
        }


        private readonly IFinancialAccountBookingFlow _bookingFlow;
    }
}