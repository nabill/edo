using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardsManagementService
    {
        Task<List<CreditCardInfo>> Get(AgentInfo agentInfo);

        Task<Result> Delete(int cardId, AgentInfo agentInfo);

        TokenizationSettings GetTokenizationSettings();

        Task<Result<string>> GetToken(int cardId, AgentInfo agentInfo);

        Task Save(CreditCardInfo cardInfo, string token, AgentInfo agentInfo);
    }
}