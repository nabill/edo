using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentSystemSettingsManagementService
    {
        Task<Result> SetAvailabilitySearchSettings(int agentId, int agencyId, AvailabilitySearchSettings settings);
    }
}