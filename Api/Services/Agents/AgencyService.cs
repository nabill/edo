using System.Collections.Generic;
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


        public async Task<Result<AgencyInfo>> GetChildAgency(int agencyId, AgentContext agent)
        {
            var agency = await _context.Agencies
                .SingleOrDefaultAsync(a => a.Id == agencyId && a.ParentId == agent.AgencyId);

            return agency is null
                ? Result.Failure<AgencyInfo>("Could not get child agency")
                : new AgencyInfo(agency.Name, agency.Id, agency.CounterpartyId);
        }


        public Task<List<AgencyInfo>> GetChildAgencies(AgentContext agent) 
            => _agencyManagementService.GetChildAgencies(agent.AgencyId);


        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly EdoContext _context;
    }
}
