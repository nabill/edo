using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentSettingsManager
    {
        Task SetAppSettings(AgentContext agentContext, JToken appSettings);

        Task<JToken> GetAppSettings(AgentContext agentContext);

        Task SetUserSettings(AgentContext agentContext, AgentUserSettings userSettings);

        Task<AgentUserSettings> GetUserSettings(AgentContext agentContext);

        AgentUserSettings DeserializeUserSettings(string settings);
    }
}