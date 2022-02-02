using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardsManagementService
    {
        TokenizationSettings GetTokenizationSettings();

        Task<Result<string>> GetToken(int cardId, AgentContext agentContext);
    }
}