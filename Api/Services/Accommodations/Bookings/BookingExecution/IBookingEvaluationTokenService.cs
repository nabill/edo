using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingEvaluationTokenService
    {
        Task<Result<AccommodationBookingInfo>> GetByEvaluationToken(string evaluationToken, string languageCode);

        Task SaveEvaluationTokenMapping(string evaluationToken, string referenceCode);
    }
}