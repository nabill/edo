using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class HttpBasedAgentContextService : IAgentContextService, IAgentContextInternal
    {
        public HttpBasedAgentContextService(EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        public async ValueTask<Result<AgentContext>> GetAgentInfo()
        {
            // TODO: Add caching
            if (!agentContext.Equals(default))
                return Result.Ok(agentContext);

            agentContext = await GetAgentInfoByIdentityHashOrId();

            return agentContext.Equals(default)
                ? Result.Failure<AgentContext>("Could not get agent data")
                : Result.Ok(agentContext);
        }


        public async ValueTask<AgentContext> GetAgent()
        {
            var (_, isFailure, agent, error) = await GetAgentInfo();
            // Normally this should not happen and such error is a signal that something is going wrong.
            if (isFailure)
                throw new UnauthorizedAccessException("Agent retrieval failure");

            return agent;
        }


        private async ValueTask<AgentContext> GetAgentInfoByIdentityHashOrId(int agentId = default)
        {
            // TODO: use counterparty information from headers to get counterparty id
            // TODO: this method assumes that only one relation exists for given AgentId, which is now not true. Needs rework. NIJO-623.
            return await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    from counterparty in _context.Counterparties.Where(c => c.Id == agency.CounterpartyId && c.IsActive)
                    where agent.IsActive && agentId.Equals(default)
                        ? agent.IdentityHash == GetUserIdentityHash()
                        : agent.Id == agentId
                    select new AgentContext(agent.Id,
                        agent.FirstName,
                        agent.LastName,
                        agent.Email,
                        agent.Title,
                        agent.Position,
                        counterparty.Id,
                        counterparty.Name,
                        agency.Id,
                        agentAgencyRelation.Type == AgentAgencyRelationTypes.Master,
                        agentAgencyRelation.InAgencyPermissions))
                .SingleOrDefaultAsync();
        }


        public async Task<List<AgentAgencyInfo>> GetAgentCounterparties()
        {
            var (_, isFailure, agentInfo, _) = await GetAgentInfo();
            if (isFailure)
                return new List<AgentAgencyInfo>(0);

            return await (
                    from cr in _context.AgentAgencyRelations
                    join ag in _context.Agencies
                        on cr.AgencyId equals ag.Id
                    join co in _context.Counterparties
                        on ag.CounterpartyId equals co.Id
                    where ag.IsActive && co.IsActive && cr.AgentId == agentInfo.AgentId
                    select new AgentAgencyInfo(
                        co.Id,
                        co.Name,
                        ag.Id,
                        ag.Name,
                        cr.Type == AgentAgencyRelationTypes.Master,
                        cr.InAgencyPermissions.ToList()))
                .ToListAsync();
        }


        //TODO TICKET https://happytravel.atlassian.net/browse/NIJO-314 
        public async ValueTask<Result<AgentContext>> SetAgentInfo(int agentId)
        {
            var agentInfo = await GetAgentInfoByIdentityHashOrId(agentId);
            if (agentInfo.Equals(default))
                return Result.Failure<AgentContext>("Could not set agent data");

            agentContext = agentInfo;
            return Result.Ok(agentContext);
        }


        public Task<bool> IsAgentAffiliatedWithAgency(int agentId, int agencyId)
            => (from ag in _context.Agencies
                join ar in _context.AgentAgencyRelations
                    on ag.Id equals ar.AgencyId
                where ag.IsActive && ar.AgentId == agentId && ar.AgencyId == agencyId
                select ag).AnyAsync();


        public Task<bool> IsAgentAffiliatedWithCounterparty(int agentId, int counterpartyId)
            => (from relation in _context.AgentAgencyRelations
                    join agency in _context.Agencies
                        on relation.AgencyId equals agency.Id
                    join cp in _context.Counterparties on agency.CounterpartyId equals cp.Id
                    where cp.IsActive && agency.IsActive && relation.AgentId == agentId && agency.CounterpartyId == counterpartyId
                    select new object())
                .AnyAsync();


        private string GetUserIdentityHash()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            return identityClaim != null
                ? HashGenerator.ComputeSha256(identityClaim)
                : string.Empty;
        }


        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private AgentContext agentContext;
    }
}