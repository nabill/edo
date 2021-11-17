using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class AgentMovementService : IAgentMovementService
    {
        public AgentMovementService(EdoContext edoContext,
            IAdminAgencyManagementService adminAgencyManagementService,  IManagementAuditService managementAuditService)
        {
            _edoContext = edoContext;
            _adminAgencyManagementService = adminAgencyManagementService;
            _managementAuditService = managementAuditService;
        }
        
        
        public async Task<Result> Move(int agentId, int sourceAgencyId, int targetAgencyId, List<int> roleIds)
        {
            return await ValidateRequest()
                .Ensure(AgentHasNoBookings, $"Agent {agentId} has bookings in agency {sourceAgencyId}")
                .Bind(UpdateAgencyRelation)
                .Bind(WriteLog)
                .Bind(DeactivateAgencyIfNeeded)
                .Bind(CreateNewMasterIfNeeded);


            async Task<Result> ValidateRequest()
            {
                if (roleIds is null || !roleIds.Any())
                    Result.Failure("Assignable role list cannot be empty");
                
                var isExists = await _edoContext.Agencies
                    .AnyAsync(a => a.Id == targetAgencyId);

                return isExists
                    ? Result.Success()
                    : Result.Failure($"Target agency {targetAgencyId} not found");
            }


            async Task<bool> AgentHasNoBookings()
                => !(await _edoContext.Bookings.AnyAsync(b => b.AgentId == agentId && b.AgencyId == sourceAgencyId));


            async Task<Result> UpdateAgencyRelation()
            {
                if (sourceAgencyId == targetAgencyId)
                    return Result.Failure($"Target agency {targetAgencyId} cannot be equal source agency {sourceAgencyId}");
                
                var relation = await _edoContext.AgentAgencyRelations
                    .SingleOrDefaultAsync(x => x.AgentId == agentId && x.AgencyId == sourceAgencyId);
                
                if (relation is null)
                    return Result.Failure($"Agent {agentId} in agency {sourceAgencyId} not found");

                var moved = new AgentAgencyRelation
                {
                    AgentId = relation.AgentId,
                    AgencyId = targetAgencyId,
                    IsActive = relation.IsActive,
                    // Moving an agent as a regular, because target agency mostly likely already contains Master
                    Type = AgentAgencyRelationTypes.Regular,
                    AgentRoleIds = roleIds.ToArray()
                };

                // Remove old record because EF Core can't update part of primary key
                _edoContext.AgentAgencyRelations.Remove(relation);
                _edoContext.AgentAgencyRelations.Add(moved);
                await _edoContext.SaveChangesAsync();
                return Result.Success();
            }


            Task<Result> WriteLog()
                => _managementAuditService.Write(ManagementEventType.AgentMovement,
                    new AgentMovedFromOneAgencyToAnother(agentId, sourceAgencyId, targetAgencyId));


            async Task<Result> DeactivateAgencyIfNeeded()
            {
                var isRelationExists = await _edoContext.AgentAgencyRelations.AnyAsync(r => r.AgencyId == sourceAgencyId);
                if (isRelationExists)
                    return Result.Success();

                return await _adminAgencyManagementService.DeactivateAgency(sourceAgencyId, "There are no agents in the agency");
            }


            async Task<Result> CreateNewMasterIfNeeded()
            {
                var isRelationExists = await _edoContext.AgentAgencyRelations.AnyAsync(r => r.AgencyId == sourceAgencyId && r.IsActive);
                if (!isRelationExists)
                    return Result.Success();

                var allAgentRoles = await _edoContext.AgentRoles.ToListAsync();
                var allPreservedRoleIds = allAgentRoles.Where(r => r.IsPreservedInAgency).Select(r => r.Id).ToList();

                var doesMasterWithPreservedRolesExist = await _edoContext.AgentAgencyRelations
                    .AnyAsync(r => r.AgencyId == sourceAgencyId
                        && r.IsActive
                        && r.Type == AgentAgencyRelationTypes.Master 
                        && allPreservedRoleIds.All(pr => r.AgentRoleIds.Contains(pr)));
                if (doesMasterWithPreservedRolesExist)
                    return Result.Success();
                
                var preservedRoleIds = allAgentRoles
                    .Where(r => r.IsPreservedInAgency)
                    .Select(r => r.Id)
                    .ToList();

                var allRolesPrivelegesCounts = allAgentRoles
                    .ToDictionary(k => k.Id, v => v.Permissions.ToList().Count);

                var allActiveAgentsRelations = await _edoContext.AgentAgencyRelations
                    .Where(rel => rel.AgencyId == sourceAgencyId && rel.IsActive)
                    .ToListAsync();

                // If there already is a master, then we just grant preserved roles to that relation.
                // If none or many, we find someone with preserved roles to make new master.
                // If none or many again, we find someone whose roles grants highest privileges sum.
                // Finally, we take the oldest agent.
                var relationToMakeMaster = allActiveAgentsRelations
                    .OrderByDescending(rel => rel.Type)
                    .ThenByDescending(rel => rel.AgentRoleIds.Count(r => preservedRoleIds.Contains(r)))
                    .ThenByDescending(rel => rel.AgentRoleIds.Sum(r => allRolesPrivelegesCounts[r]))
                    .ThenBy(rel => rel.AgentId)
                    .FirstOrDefault();

                if (relationToMakeMaster is not null)
                    await MakeMaster(relationToMakeMaster);

                return Result.Success();

                async Task MakeMaster(AgentAgencyRelation newMasterRelation)
                {
                    newMasterRelation.Type = AgentAgencyRelationTypes.Master;
                    newMasterRelation.AgentRoleIds = newMasterRelation.AgentRoleIds.Concat(preservedRoleIds).Distinct().ToArray();

                    _edoContext.Update(newMasterRelation);
                    await _edoContext.SaveChangesAsync();
                }
            }
        }


        private readonly EdoContext _edoContext;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly IManagementAuditService _managementAuditService;
    }
}