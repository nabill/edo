using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class BaseAgentPermissionManagementService
    {
        public BaseAgentPermissionManagementService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, InAgencyPermissions permissions)
        {
            return await Result.Ok()
                .Bind(GetRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission management rights")
                .Map(UpdatePermissions);


            async Task<Result<AgentAgencyRelation>> GetRelation()
            {
                var relation = await _context.AgentAgencyRelations
                    .SingleOrDefaultAsync(r => r.AgentId == agentId && r.AgencyId == agencyId);

                return relation is null
                    ? Result.Failure<AgentAgencyRelation>(
                        $"Could not find relation between the agent {agentId} and the agency {agencyId}")
                    : Result.Ok(relation);
            }


            async Task<bool> IsPermissionManagementRightNotLost(AgentAgencyRelation relation)
            {
                if (permissions.HasFlag(InAgencyPermissions.PermissionManagement))
                    return true;

                return (await _context.AgentAgencyRelations
                        .Where(r => r.AgencyId == relation.AgencyId && r.AgentId != relation.AgentId)
                        .ToListAsync())
                    .Any(c => c.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement));
            }


            async Task<List<InAgencyPermissions>> UpdatePermissions(AgentAgencyRelation relation)
            {
                relation.InAgencyPermissions = permissions;

                _context.AgentAgencyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return relation.InAgencyPermissions.ToList();
            }
        }


        private readonly EdoContext _context;
    }
}
