using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentStatusManagementService : IAgentStatusManagementService
    {
        public AgentStatusManagementService(EdoContext edoContext,
            IAgentContextService agentContextService)
        {
            _edoContext = edoContext;
            _agentContextService = agentContextService;
        }


        public async Task<Result> Enable(int agentIdToEnable)
        {
            var agentContext = await _agentContextService.GetAgent();

            return await GetAgentAgencyRelation(agentIdToEnable, agentContext.AgencyId)
                .Tap(r => SetAgentActivityStatus(r, true));
        }


        public async Task<Result> Disable(int agentIdToDisable)
        {
            var agentContext = await _agentContextService.GetAgent();

            return await CheckNotDisableYourself()
                .Bind(() => GetAgentAgencyRelation(agentIdToDisable, agentContext.AgencyId))
                .Tap(r => SetAgentActivityStatus(r, false));


            Result CheckNotDisableYourself() =>
                agentIdToDisable == agentContext.AgentId
                    ? Result.Failure("You can not disable yourself")
                    : Result.Success();
        }


        private async Task<Result<AgentAgencyRelation>> GetAgentAgencyRelation(int agentIdToEnable, int agencyId)
        {
            var relation = await _edoContext.AgentAgencyRelations.SingleOrDefaultAsync(r => r.AgentId == agentIdToEnable && r.AgencyId == agencyId);
            
            if (relation == null)
                return Result.Failure<AgentAgencyRelation>("Could not find this agent in your agency");

            return relation;
        }


        private async Task SetAgentActivityStatus(AgentAgencyRelation relation, bool isActive)
        {
            if (relation.IsActive == isActive)
                return;

            relation.IsActive = isActive;

            _edoContext.AgentAgencyRelations.Update(relation);
            await _edoContext.SaveChangesAsync();
        }


        private readonly EdoContext _edoContext;
        private readonly IAgentContextService _agentContextService;
    }
}
