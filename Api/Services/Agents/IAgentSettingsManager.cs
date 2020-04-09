using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentSettingsManager
    {
        Task SetAppSettings(AgentInfo agentInfo, JToken appSettings);

        Task<JToken> GetAppSettings(AgentInfo agentInfo);

        Task SetUserSettings(AgentInfo agentInfo, AgentUserSettings userSettings);

        Task<AgentUserSettings> GetUserSettings(AgentInfo agentInfo);
    }
}