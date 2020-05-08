using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class HttpBasedAgentContext : IAgentContext, IAgentContextInternal
    {
        public HttpBasedAgentContext(EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        public async ValueTask<Result<AgentInfo>> GetAgentInfo()
        {
            // TODO: Add caching
            if (!_agentInfo.Equals(default))
                return Result.Ok(_agentInfo);

            _agentInfo = await GetAgentInfoByIdentityHashOrId();
            
            return _agentInfo.Equals(default) 
                ? Result.Fail<AgentInfo>("Could not get agent data") 
                : Result.Ok(_agentInfo);
        }


        public async ValueTask<AgentInfo> GetAgent()
        {
            var (_, isFailure, agent, error) = await GetAgentInfo();
            // Normally this should not happen and such error is a signal that something is going wrong.
            if(isFailure)
                throw new UnauthorizedAccessException("Agent retrieval failure");

            return agent;
        }


        private async ValueTask<AgentInfo> GetAgentInfoByIdentityHashOrId(int agentId = default)
        {
            // TODO: use counterparty information from headers to get counterparty id
            return await (from agent in _context.Agents
                    from agentCounterpartyRelation in _context.AgentCounterpartyRelations.Where(r => r.AgentId == agent.Id)
                    from counterparty in _context.Counterparties.Where(c => c.Id == agentCounterpartyRelation.CounterpartyId)
                    from agency in _context.Agencies.Where(a => a.Id == agentCounterpartyRelation.AgencyId)
                    where agentId.Equals(default)
                        ? agent.IdentityHash == GetUserIdentityHash()
                        : agent.Id == agentId
                    select new AgentInfo(agent.Id,
                        agent.FirstName,
                        agent.LastName,
                        agent.Email,
                        agent.Title,
                        agent.Position,
                        counterparty.Id,
                        counterparty.Name,
                        agency.Id,
                        agentCounterpartyRelation.Type == AgentCounterpartyRelationTypes.Master,
                        agentCounterpartyRelation.InCounterpartyPermissions))
                .SingleOrDefaultAsync();
        }


        public async Task<Result<UserInfo>> GetUserInfo()
        {
            return (await GetAgentInfo())
                .OnSuccess(agent => new UserInfo(agent.AgentId, UserTypes.Agent));
        }


        public async Task<List<AgentCounterpartyInfo>> GetAgentCounterparties()
        {
            var (_, isFailure, agentInfo, _) = await GetAgentInfo();
            if (isFailure)
                return new List<AgentCounterpartyInfo>(0);

            return await (
                    from cr in _context.AgentCounterpartyRelations
                    join ag in _context.Agencies
                        on cr.AgencyId equals ag.Id
                    join co in _context.Counterparties
                        on cr.CounterpartyId equals co.Id
                    where cr.AgentId == agentInfo.AgentId
                    select new AgentCounterpartyInfo(
                        co.Id,
                        co.Name,
                        ag.Id,
                        ag.Name,
                        cr.Type == AgentCounterpartyRelationTypes.Master,
                        cr.InCounterpartyPermissions.ToList()))
                .ToListAsync();
        }


        //TODO TICKET https://happytravel.atlassian.net/browse/NIJO-314 
        public async ValueTask<Result<AgentInfo>> SetAgentInfo(int agentId)
        {
            var agentInfo = await GetAgentInfoByIdentityHashOrId(agentId);
            if (agentInfo.Equals(default))
                return Result.Fail<AgentInfo>("Could not set agent data");
            _agentInfo = agentInfo;
            return Result.Ok(_agentInfo);
        }
        
        
        private string GetUserIdentityHash()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            return identityClaim != null
                ? HashGenerator.ComputeSha256(identityClaim)
                : string.Empty;
        }
             
        
        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private AgentInfo _agentInfo;
    }
}