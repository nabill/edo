using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class PermissionChecker : IPermissionChecker
    {
        public PermissionChecker(EdoContext context,
            IDoubleFlow flow)
        {
            _context = context;
            _flow = flow;
        }


        public async ValueTask<Result> CheckInAgencyPermission(AgentContext agent, InAgencyPermissions permission)
        {
            var key = _flow.BuildKey(nameof(PermissionChecker), nameof(CheckInAgencyPermission), 
                agent.AgencyId.ToString(),
                agent.AgentId.ToString());
            
            var storedPermissions = await _flow.GetOrSetAsync(key: key, 
                getValueFunction: async () => await GetPermissions(agent.AgentId, agent.AgencyId), 
                AgentPermissionsCacheLifeTime);

            if (Equals(storedPermissions, default))
                return Result.Failure("The agent isn't affiliated with the agency");
            
            var hasPermission = storedPermissions.Any(p => p.HasFlag(permission));
            
            return hasPermission
                ? Result.Success()
                : Result.Failure($"Agent does not have the '{permission}' permission");


            // TODO: Get permissions in a single request
            async Task<List<InAgencyPermissions>> GetPermissions(int agentId, int agencyId)
            {
                var roleIds = await _context.AgentAgencyRelations
                    .Where(r => r.AgentId == agentId)
                    .Where(r => r.AgencyId == agencyId)
                    .Select(r => r.AgentRoleIds)
                    .SingleOrDefaultAsync();
            
                if (roleIds == default)
                    return default;
                    
                var permissions = await _context.AgentRoles
                    .Where(r => roleIds.Contains(r.Id))
                    .Select(r => r.Permissions)
                    .ToListAsync();
                
                return permissions;
            }
        }


        private static readonly TimeSpan AgentPermissionsCacheLifeTime = TimeSpan.FromMinutes(2);

        private readonly EdoContext _context;
        private readonly IDoubleFlow _flow;
    }
}