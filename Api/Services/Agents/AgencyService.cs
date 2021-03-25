using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyService : IAgencyService
    {
        public AgencyService(IAgentService agentService,
            IAdminAgencyManagementService agencyManagementService,
            EdoContext context)
        {
            _agentService = agentService;
            _agencyManagementService = agencyManagementService;
            _context = context;
        }


        public async Task<Result<AgencyInfo>> GetAgency(int agencyId, AgentContext agent, string languageCode = LocalizationHelper.DefaultLanguageCode)
        {
            var agentRelations = await _agentService.GetAgentRelations(agent);
            if (agentRelations.All(r => r.AgencyId != agencyId))
                return Result.Failure<AgencyInfo>("The agent is not affiliated with agency");

            var agencyInfo = await _agencyManagementService.Get(agencyId, languageCode);

            return Result.Success(agencyInfo.Value);
        }


        public Task<List<AgencyInfo>> GetChildAgencies(AgentContext agent) => _agencyManagementService.GetChildAgencies(agent.AgencyId);


        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly EdoContext _context;
    }
}
