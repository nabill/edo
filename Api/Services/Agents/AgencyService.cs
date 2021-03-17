using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

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


        public async Task<Result<AgencyInfo>> GetAgency(int agencyId, AgentContext agent)
        {
            var agentRelations = await _agentService.GetAgentRelations(agent);
            if (agentRelations.All(r => r.AgencyId != agencyId))
                return Result.Failure<AgencyInfo>("The agent is not affiliated with agency");

            var agency = await _context.Agencies.SingleAsync(a => a.Id == agencyId);

            return Result.Success(new AgencyInfo(agency.Name, agency.Id, agency.CounterpartyId));
        }


        public Task<List<AgencyInfo>> GetChildAgencies(AgentContext agent) => _agencyManagementService.GetChildAgencies(agent.AgencyId);


        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly EdoContext _context;
    }
}
