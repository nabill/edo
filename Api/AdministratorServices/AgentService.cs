using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AgentService : IAgentService
    {
        public AgentService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<List<SlimAgentInfo>>> GetAgents(int agencyId)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(a => a.Id == agencyId);
            if (agency is null)
                return Result.Failure<List<SlimAgentInfo>>($"Agency with ID {agencyId} not found");

            var relations = _context.AgentAgencyRelations
                .Where(relation => relation.AgencyId == agencyId);

            var agents = from relation in relations
                   join agent in _context.Agents on relation.AgentId equals agent.Id
                   join displayMarkupFormula in _context.DisplayMarkupFormulas on new
                   {
                       relation.AgentId,
                       relation.AgencyId
                   } equals new
                   {
                       AgentId = displayMarkupFormula.AgentId.Value,
                       AgencyId = displayMarkupFormula.AgencyId.Value
                   } into formulas
                   from formula in formulas.DefaultIfEmpty()
                   let name = $"{agent.FirstName} {agent.LastName}"
                   let created = agent.Created.ToEpochTime()
                   select new SlimAgentInfo
                   {
                       AgentId = agent.Id,
                       Name = name,
                       Created = created,
                       IsActive = relation.IsActive,
                       MarkupSettings = formula != null
                           ? formula.DisplayFormula
                           : string.Empty
                   };

            return await agents.ToListAsync();
        }
        

        private readonly EdoContext _context;
    }
}
