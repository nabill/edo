using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentRegistrationService
    {
        Task<Result> RegisterWithCounterparty(UserDescriptionInfo agentData, CounterpartyCreateRequest counterpartyData,
            string externalIdentity, string email);
    }
}