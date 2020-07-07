using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentPermissionManagementService : BaseAgentPermissionManagementService, IAgentPermissionManagementService<Agent>
    {
        public AgentPermissionManagementService(EdoContext context, IAgentContextService agentContextService) : base(context) 
            => _agentContextService = agentContextService;


        public Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, List<InAgencyPermissions> permissionList)
            => SetInAgencyPermissions(agencyId, agentId, permissionList.Aggregate((p1, p2) => p1 | p2));


        public new async Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, InAgencyPermissions permissions)
        {
            var agent = await _agentContextService.GetAgent();
            if (!agent.IsUsingAgency(agencyId))
                return Result.Failure<List<InAgencyPermissions>>("You can only edit permissions of agents from your current agency");

            return await base.SetInAgencyPermissions(agencyId, agentId, permissions);
        }


        private readonly IAgentContextService _agentContextService;
    }
}