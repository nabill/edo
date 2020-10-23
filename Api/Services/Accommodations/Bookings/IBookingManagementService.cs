using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingManagementService
    {
        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, AgentContext agent);
        
        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount);

        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId, Administrator administrator, bool requireProviderConfirmation);
        
        Task<Result<Booking, ProblemDetails>> RefreshStatus(int bookingId);
    }
}