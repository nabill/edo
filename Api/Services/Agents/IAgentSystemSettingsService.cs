using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentSystemSettingsService
    {
        public Task<AvailabilitySearchSettings?> GetAvailabilitySearchSettings(AgentContext agent);
    }
}