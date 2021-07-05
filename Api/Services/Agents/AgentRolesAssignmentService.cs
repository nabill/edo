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


        public async Task<Result> SetInAgencyRoles(int agentId, List<int> roleIdsList, AgentContext agent)
        {
            List<AgentRole> allRoles = null;

            return await GetRelation()
                .Tap(GetAllRoles)
                .Check(EnsureAdministratorRoleNotLost)
                .Ensure(AllProvidedRolesExist, "Some of specified roles do not exist")
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


            async Task GetAllRoles(AgentAgencyRelation _)
                => allRoles = await _context.AgentRoles.ToListAsync();


            async Task<Result> EnsureAdministratorRoleNotLost(AgentAgencyRelation relation)
            {
                // at least one agent with full set of preserved permissions must remain
                var preservedRoleIds = allRoles.Where(r => r.IsPreservedInAgency).Select(r => r.Id).ToList();

                if (preservedRoleIds.All(roleIdsList.Contains))
                    return Result.Success();

                return await _context.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == relation.AgencyId &&
                        r.AgentId != relation.AgentId &&
                        r.IsActive &&
                        preservedRoleIds.All(rr => r.AgentRoleIds.Contains(rr)))
                    ? Result.Success()
                    : Result.Failure("Cannot revoke last set of preserved roles");
            }


            bool AllProvidedRolesExist(AgentAgencyRelation _)
            {
                var allRolesIds = allRoles.Select(r => r.Id);
                return roleIdsList.All(allRolesIds.Contains);
            }


            async Task UpdateRoles(AgentAgencyRelation relation)
            {
                relation.AgentRoleIds = roleIdsList.ToArray();

                _context.AgentAgencyRelations.Update(relation);
                await _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
    }
}