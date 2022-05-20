using System.Threading.Tasks;
using Api.Models.Bookings;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IAgentBookingManagementService
    {
        Task<Result> Cancel(int bookingId, AgentContext agent);

        Task<Result> Cancel(string referenceCode, AgentContext agent);

        Task<Result> RefreshStatus(int bookingId, AgentContext agent);

        Task<Result<AccommodationBookingInfo>> RecalculatePrice(string referenceCode, BookingRecalculatePriceRequest request, AgentContext agent, string languageCode);
    }
}