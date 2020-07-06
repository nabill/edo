using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentPermissionManagementService<T>
    {
        Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, List<InAgencyPermissions> permissions);

        Task<Result<List<InAgencyPermissions>>> SetInAgencyPermissions(int agencyId, int agentId, InAgencyPermissions permissions);
    }
}