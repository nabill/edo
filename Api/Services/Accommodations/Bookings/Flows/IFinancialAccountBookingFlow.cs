using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Flows
{
    public interface IFinancialAccountBookingFlow
    {
        Task<Result<AccommodationBookingInfo>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp);
    }
}