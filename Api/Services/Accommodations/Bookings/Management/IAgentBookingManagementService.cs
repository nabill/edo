using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IAgentBookingManagementService
    {
        Task<Result> Cancel(int bookingId);

        Task<Result> Cancel(string referenceCode);

        Task<Result> RefreshStatus(int bookingId);
    }
}