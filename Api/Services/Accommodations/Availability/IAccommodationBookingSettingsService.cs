using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public interface IAccommodationBookingSettingsService
    {
        Task<AccommodationBookingSettings> Get(AgentContext agent);
    }
}