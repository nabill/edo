using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.CreditCards
{
    public class CreditCardsManagementService : ICreditCardsManagementService
    {
        public CreditCardsManagementService(EdoContext context, IOptions<PayfortOptions> options)
        {
            _context = context;
            _options = options.Value;
        }


        public TokenizationSettings GetTokenizationSettings() => new TokenizationSettings(_options.AccessCode, _options.Identifier, _options.TokenizationUrl);

        public Task<Result<string>> GetToken(int cardId, AgentContext agentContext)
        {
            return GetCreditCard(cardId, agentContext)
                .Map(c=> c.Token);
        }


        public Task<Result<CreditCard>> Get(int cardId, AgentContext agentContext)
        {
            return GetCreditCard(cardId, agentContext);
        }


        private async Task<Result<CreditCard>> GetCreditCard(int cardId, AgentContext agentContext)
        {
            var card = await _context.CreditCards.SingleOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                return Result.Failure<CreditCard>($"Cannot find credit card by id {cardId}");

            if (card.OwnerType == CreditCardOwnerType.Agent && card.OwnerId != agentContext.AgentId)
                Result.Failure<CreditCard>("User doesn't have access to use this credit card");

            return Result.Success(card);
        }


        private async Task<Result<CreditCard>> GetEntity(int cardId, AgentContext agentContext)
        {
            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card == null)
                return Result.Failure<CreditCard>($"Cannot find credit card by id {cardId}");

            if (card.OwnerType == CreditCardOwnerType.Agent && card.OwnerId != agentContext.AgentId)
                Result.Failure<CreditCard>("User doesn't have access to use this credit card");

            return Result.Success(card);
        }
        

        private readonly EdoContext _context;
        private readonly PayfortOptions _options;
    }
}