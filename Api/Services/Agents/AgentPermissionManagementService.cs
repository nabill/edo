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
        public AgentPermissionManagementService(EdoContext context,
            IAgentContext agentContext, IPermissionChecker permissionChecker)
        {
            _context = context;
            _agentContext = agentContext;
            _permissionChecker = permissionChecker;
        }


        public Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(int counterpartyId, int agencyId, int agentId,
            List<InCounterpartyPermissions> permissionsList) =>
            SetInCounterpartyPermissions(counterpartyId, agencyId, agentId, permissionsList.Aggregate((p1, p2) => p1 | p2));


        public async Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(int counterpartyId, int agencyId, int agentId,
            InCounterpartyPermissions permissions)
        {
            var agent = await _agentContext.GetAgent();

            return await CheckPermission()
                .OnSuccess(CheckCounterpartyAndAgency)
                .OnSuccess(GetRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission management rights")
                .OnSuccess(UpdatePermissions);

            Result CheckPermission()
            {
                if (!agent.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInAgency)
                    && !agent.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty))
                    return Result.Fail("You have no acceptance to manage agents permissions");

                return Result.Ok();
            }

            Result CheckCounterpartyAndAgency()
            {
                if (agent.CounterpartyId != counterpartyId)
                {
                    return Result.Fail("The agent isn't affiliated with the counterparty");
                }

                // TODO When agency system gets ierarchic, this needs to be changed so that agent can see agents/markups of his own agency and its subagencies
                if (!agent.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty)
                    && agent.AgencyId != agencyId)
                {
                    return Result.Fail("The agent isn't affiliated with the agency");
                }
                
                return Result.Ok();
            }

            async Task<Result<AgentCounterpartyRelation>> GetRelation()
            {
                var relation = await _context.AgentCounterpartyRelations
                    .SingleOrDefaultAsync(r => r.AgentId == agentId && r.CounterpartyId == counterpartyId && r.AgencyId == agencyId);

                return relation is null
                    ? Result.Fail<AgentCounterpartyRelation>(
                        $"Could not find relation between the agent {agentId} and the counterparty {counterpartyId}")
                    : Result.Ok(relation);
            }


            async Task<bool> IsPermissionManagementRightNotLost(AgentCounterpartyRelation relation)
            {
                if (permissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty))
                    return true;

                return (await _context.AgentCounterpartyRelations
                        .Where(r => r.CounterpartyId == relation.CounterpartyId && r.AgentId != relation.AgentId)
                        .ToListAsync())
                    .Any(c => c.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty));
            }


            async Task<List<InCounterpartyPermissions>> UpdatePermissions(AgentCounterpartyRelation relation)
            {
                relation.InCounterpartyPermissions = permissions;

                _context.AgentCounterpartyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return relation.InCounterpartyPermissions.ToList();
            }
        }


        private readonly EdoContext _context;
        private readonly IAgentContext _agentContext;
        private readonly IPermissionChecker _permissionChecker;
    }
}