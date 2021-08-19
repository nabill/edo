using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class AgentMovementService : IAgentMovementService
    {
        public AgentMovementService(EdoContext edoContext, IManagementAuditService managementAuditService)
        {
            _edoContext = edoContext;
            _managementAuditService = managementAuditService;
        }
        
        
        public async Task<Result> Move(int agentId, int sourceAgencyId, int targetAgencyId)
        {
            return await CheckTargetAgencyExists()
                .Bind(UpdateAgencyRelation)
                .Bind(WriteLog);


            async Task<Result> CheckTargetAgencyExists()
            {
                var isExists = await _edoContext.Agencies
                    .AnyAsync(a => a.Id == targetAgencyId);

                return isExists
                    ? Result.Success()
                    : Result.Failure($"Target agency {targetAgencyId} not found");
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
                    Type = relation.Type,
                    AgentRoleIds = relation.AgentRoleIds
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
        }


        private readonly EdoContext _edoContext;
        private readonly IManagementAuditService _managementAuditService;
    }
}