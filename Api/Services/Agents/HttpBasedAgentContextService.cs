using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class HttpBasedAgentContextService : IAgentContextService, IAgentContextInternal
    {
        public HttpBasedAgentContextService(EdoContext context,
            ITokenInfoAccessor tokenInfoAccessor,
            IHttpContextAccessor httpContextAccessor,
            IMemoryFlow flow)
        {
            _context = context;
            _tokenInfoAccessor = tokenInfoAccessor;
            _httpContextAccessor = httpContextAccessor;
            _flow = flow;
        }


        public async ValueTask<Result<AgentContext>> GetAgentInfo()
        {
            if (!_currentAgentContext.Equals(default))
                return _currentAgentContext;

            _currentAgentContext = await GetAgentContext();

            return _currentAgentContext.Equals(default)
                ? Result.Failure<AgentContext>("Could not get agent data")
                : _currentAgentContext;
        }


        public async Task RefreshAgentContext()
        {
            var identityClaim = _tokenInfoAccessor.GetIdentity();
            var identityHash = identityClaim is not null
                ? HashGenerator.ComputeSha256(identityClaim)
                : string.Empty;

            var key = GetKey(identityHash);

            _flow.Remove(key);

            _currentAgentContext = await _flow.GetOrSetAsync(
                key: key,
                getValueFunction: async () => await GetAgentInfoByIdentityHash(identityHash),
                AgentContextCacheLifeTime);
        }


        public async Task<ContractKind?> GetContractKind()
        {
            var agent = await GetAgent();

            return await _context.Agencies
                .Where(a => a.Id == agent.AgencyId)
                .Select(a => a.ContractKind)
                .SingleOrDefaultAsync();
        }


        private async Task<AgentContext> GetAgentContext()
        {
            var clientId = _tokenInfoAccessor.GetClientId();

            if (clientId == TravelGateConnectorClientName)
                return await GetForApiClient();

            return await GetForFrontendClient();


            async Task<AgentContext> GetForApiClient()
            {
                var name = GetHeaderValue("X-Api-Client-Name");
                var password = GetHeaderValue("X-Api-Client-Password");

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                    return default;

                var passwordHash = HashGenerator.ComputeSha256(password);
                var key = GetKey($"{name}{passwordHash}");

                return await _flow.GetOrSetAsync(
                    key,
                    async () => await GetAgentInfoByApiClientCredentials(name, passwordHash),
                    AgentContextCacheLifeTime);

                string GetHeaderValue(string header)
                    => _httpContextAccessor.HttpContext?.Request.Headers[header];
            }


            async Task<AgentContext> GetForFrontendClient()
            {
                var identityClaim = _tokenInfoAccessor.GetIdentity();
                var identityHash = identityClaim is not null
                    ? HashGenerator.ComputeSha256(identityClaim)
                    : string.Empty;

                var key = GetKey(identityHash);

                return await _flow.GetOrSetAsync(
                    key: key,
                    getValueFunction: async () => await GetAgentInfoByIdentityHash(identityHash),
                    AgentContextCacheLifeTime);
            }
        }


        private string GetKey(string name)
            => _flow.BuildKey(nameof(HttpBasedAgentContextService), nameof(GetAgentInfo), name);


        public async ValueTask<AgentContext> GetAgent()
        {
            var (_, isFailure, agent, error) = await GetAgentInfo();
            // Normally this should not happen and such error is a signal that something is going wrong.
            if (isFailure)
                throw new UnauthorizedAccessException("Agent retrieval failure");

            return agent;
        }


        private async ValueTask<AgentContext> GetAgentInfoByIdentityHash(string identityHash)
        {
            // TODO: this method assumes that only one relation exists for given AgentId, which is now not true. Needs rework. NIJO-623.
            // TODO: there are too many requests to database, find a way to get rid of this query
            var inAgencyPermissions = await GetInAgencyPermissionsByIdentityHash(identityHash);
            return await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    from country in _context.Countries.Where(c => c.Code == agency.CountryCode)
                    where agentAgencyRelation.IsActive && agent.IdentityHash == identityHash
                    select new AgentContext(agent.Id,
                        agent.FirstName,
                        agent.LastName,
                        agent.Email,
                        agent.Title,
                        agent.Position,
                        agency.Id,
                        agency.Name,
                        agentAgencyRelation.Type == AgentAgencyRelationTypes.Master,
                        inAgencyPermissions,
                        agency.CountryHtId,
                        agency.LocalityHtId,
                        country.Code,
                        country.MarketId,
                        agency.Ancestors))
                .SingleOrDefaultAsync();
        }


        private async ValueTask<AgentContext> GetAgentInfoByApiClientCredentials(string name, string passwordHash)
        {
            // TODO: there are too many requests to database, find a way to get rid of this query
            var inAgencyPermissions = await GetInAgencyPermissionsByApiClientCredentials(name, passwordHash);
            return await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from apiClient in _context.ApiClients.Where(a => a.Name == name && a.PasswordHash == passwordHash)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    from country in _context.Countries.Where(c => c.Code == agency.CountryCode)
                    where agentAgencyRelation.IsActive && agent.Id == apiClient.AgentId && agency.Id == apiClient.AgencyId
                    select new AgentContext(agent.Id,
                        agent.FirstName,
                        agent.LastName,
                        agent.Email,
                        agent.Title,
                        agent.Position,
                        agency.Id,
                        agency.Name,
                        agentAgencyRelation.Type == AgentAgencyRelationTypes.Master,
                        inAgencyPermissions,
                        agency.CountryHtId,
                        agency.LocalityHtId,
                        country.Code,
                        country.MarketId,
                        agency.Ancestors))
                .SingleOrDefaultAsync();
        }


        private async Task<InAgencyPermissions> GetInAgencyPermissionsByIdentityHash(string identityHash)
        {
            var agentRoleIds = await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    where agent.IdentityHash == identityHash
                    select agentAgencyRelation.AgentRoleIds)
                .SingleOrDefaultAsync();

            return await GetAggregateInAgencyPermissions(agentRoleIds);
        }


        private async Task<InAgencyPermissions> GetInAgencyPermissionsByApiClientCredentials(string name, string passwordHash)
        {
            var agentRoleIds = await (from agent in _context.Agents
                    from agentAgencyRelation in _context.AgentAgencyRelations.Where(r => r.AgentId == agent.Id)
                    from apiClient in _context.ApiClients.Where(a => a.Name == name && a.PasswordHash == passwordHash)
                    from agency in _context.Agencies.Where(a => a.Id == agentAgencyRelation.AgencyId && a.IsActive)
                    where agentAgencyRelation.IsActive && agent.Id == apiClient.AgentId && agency.Id == apiClient.AgencyId
                    select agentAgencyRelation.AgentRoleIds)
                .SingleOrDefaultAsync();

            return await GetAggregateInAgencyPermissions(agentRoleIds);
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


        private const string TravelGateConnectorClientName = "travelgate_channel";

        private static readonly TimeSpan AgentContextCacheLifeTime = TimeSpan.FromMinutes(2);

        private readonly EdoContext _context;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryFlow _flow;
        private AgentContext _currentAgentContext;
    }
}