using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IChangeAgentAgencyService
    {
        Task<Result> Move(int agentId, int sourceAgencyId, int destinationAgencyId);
    }
}