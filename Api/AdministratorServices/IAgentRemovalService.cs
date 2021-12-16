using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentRemovalService
    {
        Task<Result> RemoveFromAgency(int agentId, int agencyId);
    }
}