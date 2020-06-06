using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingService
    {
        Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, string languageCode);

        Task<Result<AccommodationBookingInfo, ProblemDetails>> Finalize(string referenceCode, AgentInfo agent, string languageCode);
        
        Task ProcessResponse(BookingDetails bookingResponse, Data.Booking.Booking booking);

        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, AgentInfo agent);
        
        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount);
        
        Task<Result<BookingDetails, ProblemDetails>> RefreshStatus(int bookingId);
    }
}