using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentSystemSettingsManagementService
    {
        Task<Result> SetAvailabilitySearchSettings(int agentId, int agencyId, AgentAccommodationBookingSettings settings);

        Task<Result<AgentAccommodationBookingSettings>> GetAvailabilitySearchSettings(int agentId, int agencyId);

        Task<Result> DeleteAvailabilitySearchSettings(int agentId, int agencyId);
    }
}