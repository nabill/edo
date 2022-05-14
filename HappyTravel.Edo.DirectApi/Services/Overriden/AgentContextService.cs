using System;
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

namespace HappyTravel.Edo.DirectApi.Services.Overriden
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
            var data = await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from agentDirectApiRelation in _context.AgentDirectApiClientRelations.Where(a => a.AgentId == agentAgencyRelation.AgentId
                        && a.AgencyId == agentAgencyRelation.AgencyId && a.DirectApiClientId == clientId)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    from country in _context.Countries.Where(c => c.Code == agency.CountryCode)
                    select new
                    {
                        AgentId = agent.Id,
                        FirstName = agent.FirstName,
                        LastName = agent.LastName,
                        Email = agent.Email,
                        Title = agent.Title,
                        Position = agent.Position,
                        AgencyId = agency.Id,
                        AgencyName = agency.Name,
                        IsMaster = agentAgencyRelation.Type == AgentAgencyRelationTypes.Master,
                        agentAgencyRelation.AgentRoleIds,
                        CountryHtId = agency.CountryHtId,
                        LocalityHtId = agency.LocalityHtId,
                        CountryCode = country.Code,
                        MarketId = country.MarketId,
                        AgencyAncestors = agency.Ancestors,
                        AgencyContractKind = agency.ContractKind
                    })
                .SingleOrDefaultAsync();

            if (data is null)
                return default;

            return new AgentContext(agentId: data.AgentId,
                firstName: data.FirstName,
                lastName: data.LastName,
                email: data.Email,
                title: data.Title,
                position: data.Position,
                agencyId: data.AgencyId,
                agencyName: data.AgencyName,
                isMaster: data.IsMaster,
                inAgencyPermissions: await GetAggregateInAgencyPermissions(data.AgentRoleIds),
                countryHtId: data.CountryHtId,
                localityHtId: data.LocalityHtId,
                countryCode: data.CountryCode,
                marketId: data.MarketId,
                agencyAncestors: data.AgencyAncestors,
                agencyContractKind: data.AgencyContractKind
                );
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