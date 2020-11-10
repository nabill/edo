using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentAgencyEnablementService : IAgentAgencyEnablementService
    {
        public AgentAgencyEnablementService(EdoContext edoContext)
        {
            _edoContext = edoContext;
        }


        public async Task<Result> Enable(int agencyId, int agentIdToEnable, AgentContext agentContext)
        {
            return await Result.Success()
                .Ensure(() => agentContext.IsUsingAgency(agencyId), "You can only enable agents in an agency you are currently using")
                .Bind(() => GetAgentAgencyRelation(agentIdToEnable, agencyId))
                .Ensure(r => !r.IsEnabled, "Agent is already enabled")
                .Tap(r => SetAgentAgencyEnablement(r, true));
        }


        public async Task<Result> Disable(int agencyId, int agentIdToDisable, AgentContext agentContext)
        {
            return await Result.Success()
                .Ensure(() => agentContext.IsUsingAgency(agencyId), "You can only disable agents in an agency you are currently using")
                .Ensure(() => agentIdToDisable != agentContext.AgentId, "You can not disable yourself")
                .Bind(() => GetAgentAgencyRelation(agentIdToDisable, agencyId))
                .Ensure(r => r.IsEnabled, "Agent is already disabled")
                .Tap(r => SetAgentAgencyEnablement(r, false));
        }


        private async Task<Result<AgentAgencyRelation>> GetAgentAgencyRelation(int agentIdToEnable, int agencyId)
        {
            var relation = await _edoContext.AgentAgencyRelations.SingleOrDefaultAsync(r => r.AgentId == agentIdToEnable && r.AgencyId == agencyId);
            
            if (relation == null)
                return Result.Failure<AgentAgencyRelation>("Could not find the given agent in this agency");

            return relation;
        }


        private async Task SetAgentAgencyEnablement(AgentAgencyRelation relation, bool isEnabled)
        {
            relation.IsEnabled = isEnabled;

            _edoContext.AgentAgencyRelations.Update(relation);
            await _edoContext.SaveChangesAsync();
        }


        private readonly EdoContext _edoContext;
    }
}
