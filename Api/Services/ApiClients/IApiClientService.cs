using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.ApiClients;

namespace HappyTravel.Edo.Api.Services.ApiClients
{
    public interface IApiClientService
    {
        public Task<Result<ApiClientInfo>> GetCurrent(AgentContext agent);

        public Task<GeneratedApiClient> GenerateApiClient(AgentContext agent);
    }
}