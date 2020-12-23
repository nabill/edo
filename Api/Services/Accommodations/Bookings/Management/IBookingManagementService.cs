using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingManagementService
    {
        Task<Result<Unit, ProblemDetails>> Cancel(int bookingId, AgentContext agent);
        
        Task<Result<Unit, ProblemDetails>> Cancel(int bookingId, ServiceAccount serviceAccount);

        Task<Result<Unit, ProblemDetails>> Cancel(int bookingId, Administrator administrator, bool requireSupplierConfirmation);
        
        Task<Result<Unit, ProblemDetails>> RefreshStatus(int bookingId);
    }
}