using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.AdministratorServices
{
    public class AgentPermissionManagementAdministratorService : BaseAgentPermissionManagementService, IAgentPermissionManagementService<Administrator>
    {
        public AgentPermissionManagementAdministratorService(EdoContext context) : base(context)
        { }


        public Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, List<InAgencyPermissions> permissions) 
            => SetInAgencyPermissions(agencyId, agentId, permissions.Aggregate((p1, p2) => p1 | p2));


        public new Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, InAgencyPermissions permissions)
        {
            return base.SetInAgencyPermissions(agencyId, agentId, permissions);
        }
    }
}
