using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentContextService
    {
        ValueTask<AgentContext> GetAgent();
        Task<ContractKind?> GetContractKind();
    }
}