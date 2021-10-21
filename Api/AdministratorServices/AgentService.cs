using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Agents;

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


        public async Task<Result> BindDirectApiClient(int agentId, string clientId)
        {
            var agentHasRelation = await _context.AgentDirectApiClientRelations
                .AnyAsync(r => r.AgentId == agentId);
            
            if (agentHasRelation)
                return Result.Failure($"Agent `{agentId}` already bounded");

            var dacHasRelation = await _context.AgentDirectApiClientRelations
                .AnyAsync(r => r.DirectApiClientId == clientId);
            
            if (dacHasRelation)
                return Result.Failure($"Direct api client {clientId} already bounded");
            
            _context.AgentDirectApiClientRelations.Add(new AgentDirectApiClientRelation
            {
                AgentId = agentId,
                DirectApiClientId = clientId
            });
            await _context.SaveChangesAsync();
            return Result.Success();
        }


        public async Task<Result> UnbindDirectApiClient(int agentId, string clientId)
        {
            var relation = await _context.AgentDirectApiClientRelations
                .SingleOrDefaultAsync(r => r.AgentId == agentId && r.DirectApiClientId == clientId);
            
            if (relation is null)
                return Result.Failure($"Binding between agent `{agentId}` and `{clientId}` not found");
            
            _context.Remove(relation);
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        private readonly EdoContext _context;
    }
}
