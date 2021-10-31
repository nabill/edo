﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class AgentContextService : IAgentContextService
    {
        public AgentContextService(ITokenInfoAccessor tokenInfoAccessor, IMemoryFlow flow, EdoContext context)
        {
            _tokenInfoAccessor = tokenInfoAccessor;
            _flow = flow;
            _context = context;
        }


        public async ValueTask<AgentContext> GetAgent()
        {
            var clientId = _tokenInfoAccessor.GetClientId();
            var key = GetKey(clientId);

            return await _flow.GetOrSetAsync(
                key: key,
                getValueFunction: async () => await GetAgentContextByDirectApiClientId(clientId),
                AgentContextCacheLifeTime);
        }
        
        
        private string GetKey(string name) 
            => _flow.BuildKey(nameof(AgentContextService), nameof(GetAgent), name);
        
        
        private async ValueTask<AgentContext> GetAgentContextByDirectApiClientId(string clientId)
        {
            var data =  await (from agent in _context.Agents
                    from agentDirectApiRelation in _context.AgentDirectApiClientRelations.Where(a => a.AgentId == agent.Id && a.DirectApiClientId == clientId)
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    from counterparty in _context.Counterparties.Where(c => c.Id == agency.CounterpartyId)
                    select new {
                        AgentId = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Email = agent.Email,
                        Title = agent.Title,
                        Position = agent.Position,
                        CounterpartyId = counterparty.Id,
                        CounterpartyName = counterparty.Name,
                        AgencyId = agency.Id,
                        IsMaster = agentAgencyRelation.Type == AgentAgencyRelationTypes.Master,
                        agentAgencyRelation.AgentRoleIds})
                .SingleOrDefaultAsync();

            if (data is null)
                return default;

            return new AgentContext(agentId: data.AgentId,
                firstName: data.FirstName,
                lastName: data.LastName,
                email: data.Email,
                title: data.Title,
                position: data.Position,
                counterpartyId: data.CounterpartyId,
                counterpartyName: data.CounterpartyName,
                agencyId: data.AgencyId,
                isMaster: data.IsMaster,
                inAgencyPermissions: await GetAggregateInAgencyPermissions(data.AgentRoleIds));
        }
        
        
        private async Task<InAgencyPermissions> GetAggregateInAgencyPermissions(int[] agentRoleIds)
        {
            if (agentRoleIds is null || !agentRoleIds.Any())
                return 0;
            
            var permissionList = await (from agentRole in _context.AgentRoles
                    where agentRoleIds.Contains(agentRole.Id)
                    select agentRole.Permissions)
                .ToListAsync();

            return permissionList.Aggregate((a, b) => a | b);
        }

        
        private static readonly TimeSpan AgentContextCacheLifeTime = TimeSpan.FromMinutes(2);
        

        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IMemoryFlow _flow;
        private readonly EdoContext _context;
    }
}