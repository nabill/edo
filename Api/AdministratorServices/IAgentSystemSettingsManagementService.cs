using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentSystemSettingsManagementService
    {
        Task<Result> SetAvailabilitySearchSettings(int agentId, int agencyId, AgentAccommodationBookingSettingsInfo settings);

        Task<Result<AgentAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agentId, int agencyId);

        Task<Result> DeleteAvailabilitySearchSettings(int agentId, int agencyId);
    }
}