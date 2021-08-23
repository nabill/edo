using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using HappyTravel.Edo.Data.Payments;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public interface ICreditCardsManagementService
    {
        Task<List<CreditCardInfo>> Get(AgentContext agentContext);
        
        Task<Result<CreditCard>> Get(int cardId, AgentContext agentContext);

        Task<Result> Delete(int cardId, AgentContext agentContext);

        TokenizationSettings GetTokenizationSettings();

        Task<Result<string>> GetToken(int cardId, AgentContext agentContext);

        Task Save(CreditCardInfo cardInfo, string token, AgentContext agentContext);
    }
}