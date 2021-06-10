using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentPermissionManagementService : IAgentPermissionManagementService
    {
        public AgentPermissionManagementService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agentId, List<InAgencyPermissions> permissionsList, AgentContext agent)
        {
            return await Result.Success()
                .Bind(GetRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission or status management rights")
                .Map(AddBundledPermissions)
                .Map(UpdatePermissions);

            async Task<Result<AgentAgencyRelation>> GetRelation()
            {
                var relation = await _context.AgentAgencyRelations
                    .SingleOrDefaultAsync(r => r.AgentId == agentId && r.AgencyId == agent.AgencyId);

                return relation is null
                    ? Result.Failure<AgentAgencyRelation>(
                        $"Could not find relation between the agent {agentId} and the agency {agent.AgencyId}")
                    : Result.Success(relation);
            }


            async Task<bool> IsPermissionManagementRightNotLost(AgentAgencyRelation relation)
            {
                if (permissionsList.Contains(InAgencyPermissions.PermissionManagement) && permissionsList.Contains(InAgencyPermissions.AgentStatusManagement))
                    return true;

                return await _context.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == relation.AgencyId && r.AgentId != relation.AgentId && r.IsActive &&
                        r.InAgencyPermissions.HasFlag(InAgencyPermissions.PermissionManagement) &&
                        r.InAgencyPermissions.HasFlag(InAgencyPermissions.AgentStatusManagement));
            }


            (AgentAgencyRelation, List<InAgencyPermissions>) AddBundledPermissions(AgentAgencyRelation relation)
            {
                List<InAgencyPermissions> additionalPermissions = new List<InAgencyPermissions>();

                foreach (var permission in permissionsList)
                {
                    if (_bundledPermissions.TryGetValue(permission, out var bundledPermissions))
                        additionalPermissions.AddRange(bundledPermissions);
                }

                return (relation, permissionsList.Union(additionalPermissions).ToList());
            }


            async Task<List<InAgencyPermissions>> UpdatePermissions((AgentAgencyRelation Relation, List<InAgencyPermissions> Permissions) values)
            {
                var permissions = values.Permissions.Any()
                    ? values.Permissions.Aggregate((p1, p2) => p1 | p2)
                    : default;

                values.Relation.InAgencyPermissions = permissions;

                _context.AgentAgencyRelations.Update(values.Relation);
                await _context.SaveChangesAsync();

                return values.Relation.InAgencyPermissions.ToList();
            }
        }


        private readonly EdoContext _context;
        private readonly Dictionary<InAgencyPermissions, List<InAgencyPermissions>> _bundledPermissions = new ()
        {
            [InAgencyPermissions.PermissionManagement] = new () { InAgencyPermissions.ObserveAgents, InAgencyPermissions.AgentStatusManagement },
            [InAgencyPermissions.AgentStatusManagement] = new () { InAgencyPermissions.ObserveAgents },
            [InAgencyPermissions.MarkupManagement] = new () { InAgencyPermissions.ObserveMarkup },
            [InAgencyPermissions.AgencyToChildTransfer] = new() { InAgencyPermissions.ObserveChildAgencies },
        };
    }
}