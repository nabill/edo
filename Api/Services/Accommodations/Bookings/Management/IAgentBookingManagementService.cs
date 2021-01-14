using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IAgentBookingManagementService
    {
        Task<Result> Cancel(int bookingId, AgentContext agent);

        Task<Result> RefreshStatus(int bookingId, AgentContext agent);
    }
}