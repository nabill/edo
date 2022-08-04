using System.Threading.Tasks;
using Api.Services.Internal;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentSystemSettingsService : IAgentSystemSettingsService
    {
        public AgentSystemSettingsService(EdoContext context,
            IInternalSystemSettingsService internalSystemSettingsService)
        {
            _context = context;
            _internalSystemSettingsService = internalSystemSettingsService;
        }

        public async Task<Maybe<AgentAccommodationBookingSettings>> GetAccommodationBookingSettings(AgentContext agent)
        {
            var (_, isFailure, settings) = await _internalSystemSettingsService
                .GetAgentMaterializedSearchSettings(agent.AgentId, agent.AgencyId);
            if (isFailure)
                return Maybe<AgentAccommodationBookingSettings>.None;

            return settings;
        }


        private readonly EdoContext _context;
        private readonly IInternalSystemSettingsService _internalSystemSettingsService;
    }
}