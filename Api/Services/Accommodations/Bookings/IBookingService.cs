using System;
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
        Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, AgentContext agent, string languageCode);

        Task<Result<AccommodationBookingInfo, ProblemDetails>> Finalize(string referenceCode, AgentContext agentContext, string languageCode);

        Task<Result<AccommodationBookingInfo, ProblemDetails>> BookByAccount(AccommodationBookingRequest bookingRequest, AgentContext agentContext, string languageCode, string clientIp);
        
        Task ProcessResponse(BookingDetails bookingResponse, Data.Booking.Booking booking);

        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, AgentContext agent);
        
        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount);

        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, Administrator administrator, bool requireProviderConfirmation);
        
        Task<Result<BookingDetails, ProblemDetails>> RefreshStatus(int bookingId);
    }
}