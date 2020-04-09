using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentSettingsManager : IAgentSettingsManager
    {
        public AgentSettingsManager(EdoContext context, IJsonSerializer serializer)
        {
            _context = context;
            _serializer = serializer;
        }


        public async Task SetAppSettings(AgentInfo agentInfo, JToken appSettings)
        {
            var agent = await GetAgent(agentInfo);
            agent.AppSettings = appSettings.ToString(Formatting.None);
            _context.Update(agent);
            await _context.SaveChangesAsync();
        }


        public async Task<JToken> GetAppSettings(AgentInfo agentInfo)
        {
            var settings = await _context.Agents
                    .Where(a => a.Id == agentInfo.AgentId)
                    .Select(a => a.AppSettings)
                    .SingleOrDefaultAsync() ?? Infrastructure.Constants.Common.EmptyJsonFieldValue;

            return JToken.Parse(settings);
        }


        public async Task SetUserSettings(AgentInfo agentInfo, AgentUserSettings userSettings)
        {
            var agent = await GetAgent(agentInfo);
            agent.UserSettings = _serializer.SerializeObject(userSettings);
            _context.Update(agent);
            await _context.SaveChangesAsync();
        }


        public async Task<AgentUserSettings> GetUserSettings(AgentInfo agentInfo)
        {
            var settings = await _context.Agents
                .Where(a => a.Id == agentInfo.AgentId)
                .Select(a => a.UserSettings)
                .SingleOrDefaultAsync();

            return settings == default
                ? default
                : _serializer.DeserializeObject<AgentUserSettings>(settings);
        }


        private Task<Agent> GetAgent(AgentInfo agentInfo) => _context.Agents
            .SingleOrDefaultAsync(a => a.Id == agentInfo.AgentId);


        private readonly EdoContext _context;
        private readonly IJsonSerializer _serializer;
    }
}