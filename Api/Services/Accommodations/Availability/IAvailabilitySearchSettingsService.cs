using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAvailabilitySearchSettingsService
    {
        Task<AccommodationBookingSettings> Get(AgentContext agent);
    }
}