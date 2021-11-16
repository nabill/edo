using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentRegistrationService
    {
        Task<Result> RegisterWithAgency(UserDescriptionInfo agentData, RegistrationAgencyInfo registrationAgencyInfo, string externalIdentity,
            string email);
    }
}