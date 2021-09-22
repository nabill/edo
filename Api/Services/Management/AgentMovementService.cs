using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class AgentMovementService : IAgentMovementService
    {
        public AgentMovementService(EdoContext edoContext, ICounterpartyManagementService counterpartyManagementService,
            IAdminAgencyManagementService adminAgencyManagementService,  IManagementAuditService managementAuditService)
        {
            _edoContext = edoContext;
            _counterpartyManagementService = counterpartyManagementService;
            _adminAgencyManagementService = adminAgencyManagementService;
            _managementAuditService = managementAuditService;
        }
        
        
        public async Task<Result> Move(int agentId, int sourceAgencyId, int targetAgencyId, List<int> roleIds)
        {
            return await ValidateRequest()
                .Ensure(AgentHasNoBookings, $"Agent {agentId} has bookings in agency {sourceAgencyId}")
                .Bind(GetMasterAgent)
                .Check(_ => UpdateAgencyRelation())
                .Check(_ => WriteLog())
                .Check(_ => DeactivateAgencyIfNeeded())
                .Check(DeactivateCounterpartyIfNeeded);


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


            async Task<Result<MasterAgentContext>> GetMasterAgent()
            {
                var sourceAgency = await _edoContext.Agencies.SingleOrDefaultAsync(a => a.Id == sourceAgencyId);

                var (_, isFailure, master, error) = await _counterpartyManagementService.GetRootAgencyMasterAgent(sourceAgency.CounterpartyId);
                if (isFailure)
                    return Result.Failure<MasterAgentContext>(error);

                return master;
            }


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


            async Task<Result> DeactivateCounterpartyIfNeeded(MasterAgentContext masterAgentContext)
            {
                var sourceAgency = await _edoContext.Agencies.SingleOrDefaultAsync(a => a.Id == sourceAgencyId);

                var isActiveAgencyExists = await _edoContext.Agencies.AnyAsync(a => a.CounterpartyId == sourceAgency.CounterpartyId && a.IsActive);
                if (isActiveAgencyExists)
                    return Result.Success();

                return await _counterpartyManagementService.DeactivateCounterparty(sourceAgency.CounterpartyId, 
                    "There are no active agencies in the counterparty", masterAgentContext);
            }
        }


        private readonly EdoContext _edoContext;
        private readonly ICounterpartyManagementService _counterpartyManagementService;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
        private readonly IManagementAuditService _managementAuditService;
    }
}