using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentRolesAssignmentService : IAgentRolesAssignmentService
    {
        public AgentRolesAssignmentService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result> SetInAgencyRoles(int agentId, List<string> roleNamesList, AgentContext agent)
        {
            return await GetRelation()
                .Check(EnsureAdministratorRoleNotLost)
                .Tap(UpdateRoles);


            async Task<Result<AgentAgencyRelation>> GetRelation()
            {
                var relation = await _context.AgentAgencyRelations
                    .SingleOrDefaultAsync(r => r.AgentId == agentId && r.AgencyId == agent.AgencyId);

                return relation is null
                    ? Result.Failure<AgentAgencyRelation>(
                        $"Could not find relation between the agent {agentId} and the agency {agent.AgencyId}")
                    : Result.Success(relation);
            }


            async Task<Result> EnsureAdministratorRoleNotLost(AgentAgencyRelation relation)
            {
                if (roleNamesList.Any(r => r.ToLower() == AgencyAdministratorRoleName.ToLower()))
                    return Result.Success();

                var adminRole = await _context.AgentRoles.SingleOrDefaultAsync(r => r.Name.ToLower() == AgencyAdministratorRoleName.ToLower());
                if (adminRole == default)
                    return Result.Failure("Could not find the administrator role");

                return await _context.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == relation.AgencyId &&
                        r.AgentId != relation.AgentId &&
                        r.IsActive &&
                        r.AgentRoleIds.Contains(adminRole.Id))
                    ? Result.Success()
                    : Result.Failure("Cannot revoke last administrator role");
            }


            async Task UpdateRoles(AgentAgencyRelation relation)
            {
                var roleLowerNamesList = roleNamesList.Select(r => r.ToLower());
                var roleIds = await _context.AgentRoles
                    .Where(r => roleLowerNamesList.Contains(r.Name.ToLower()))
                    .Select(r => r.Id)
                    .ToArrayAsync();

                relation.AgentRoleIds = roleIds;

                _context.AgentAgencyRelations.Update(relation);
                await _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
        
        private const string AgencyAdministratorRoleName = "Agency administrator";
    }
}