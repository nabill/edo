using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IAgentMovementService
    {
        Task<Result> Move(int agentId, int sourceAgencyId, int targetAgencyId, List<int> roleIds);
    }
}