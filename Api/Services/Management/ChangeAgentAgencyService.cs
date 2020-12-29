using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.AuditEvents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class ChangeAgentAgencyService : IChangeAgentAgencyService
    {
        public ChangeAgentAgencyService(EdoContext edoContext, IManagementAuditService managementAuditService)
        {
            _edoContext = edoContext;
            _managementAuditService = managementAuditService;
        }
        
        
        public async Task<Result> Move(int agentId, int sourceAgencyId, int destinationAgencyId)
        {
            return await IsAgentExist()
                .Bind(UpdateAgencyRelation)
                .Bind(WriteLog);


            async Task<Result> IsAgentExist()
            {
                var isExists = await _edoContext.Agents
                    .AnyAsync(a => a.Id == agentId);

                return isExists
                    ? Result.Success()
                    : Result.Failure($"Agent {agentId} not found");
            }


            async Task<Result> UpdateAgencyRelation()
            {
                if (sourceAgencyId == destinationAgencyId)
                    return Result.Failure($"Destination agency {destinationAgencyId} cannot be equal source agency {sourceAgencyId}");
                
                var relation = await _edoContext.AgentAgencyRelations
                    .SingleOrDefaultAsync(x => x.AgentId == agentId && x.AgencyId == sourceAgencyId);
                
                if (relation is null)
                    return Result.Failure($"Agent {agentId} in agency {sourceAgencyId} not found");

                var moved = new AgentAgencyRelation
                {
                    AgentId = relation.AgentId,
                    AgencyId = destinationAgencyId,
                    InAgencyPermissions = relation.InAgencyPermissions,
                    IsActive = relation.IsActive,
                    Type = relation.Type
                };

                // Remove old record because EF Core can't update part of primary key
                _edoContext.AgentAgencyRelations.Remove(relation);
                _edoContext.AgentAgencyRelations.Add(moved);
                await _edoContext.SaveChangesAsync();
                return Result.Success();
            }


            Task<Result> WriteLog()
                => _managementAuditService.Write(ManagementEventType.MoveAgentFromOneAgencyToAnother, 
                    new AgentMovedFromOneAgencyToAnother(agentId, sourceAgencyId, destinationAgencyId));
        }


        private readonly EdoContext _edoContext;
        private readonly IManagementAuditService _managementAuditService;
    }
}