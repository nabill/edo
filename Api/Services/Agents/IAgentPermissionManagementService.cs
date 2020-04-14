using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentPermissionManagementService
    {
        Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(
            int counterpartyId, int agencyId, int agentId, List<InCounterpartyPermissions> permissions);

        Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(
            int counterpartyId, int agencyId, int agentId, InCounterpartyPermissions permissions);
    }
}